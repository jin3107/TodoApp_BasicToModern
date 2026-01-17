import { useState, useEffect, useCallback } from "react";
import {
  Button,
  Input,
  Table,
  Select,
  Modal,
  Form,
  Descriptions,
  Spin,
  Typography,
  Tag,
  Space,
  Row,
  Col,
  Card,
  Statistic,
  DatePicker,
  Switch,
  Dropdown,
  App,
} from "antd";
import type { MenuProps } from "antd";
import {
  PlusOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  ExclamationCircleOutlined,
  SearchOutlined,
  SortAscendingOutlined,
  SortDescendingOutlined,
  MoreOutlined,
  EyeOutlined,
  EditOutlined,
  DeleteOutlined,
} from "@ant-design/icons";
import dayjs, { Dayjs } from "dayjs";
import { Tier } from "../../commons";
import type { Filter, SearchRequest, SearchResponse, TodoItemRequest, TodoItemResponse } from "../../interfaces";
import { createTodoItem, deleteTodoItem, getTodoItemById, searchTodoItems, updateTodoItem } from "../../apis/todoItemAPI";
import { PriorityTag, StatusTag, useDateFormatter } from "../../components";
import "./style.scss";

const { TextArea } = Input;
const { Option } = Select;
const { Text } = Typography;

type Mode = "create" | "detail" | "update";

interface TodoItemData {
  id: string;
  title: string;
  description: string;
  dueDate: string;
  isCompleted: boolean;
  priority: Tier;
  completedOn?: string;
}

const TodoItems = () => {
  const { modal, message: messageApi } = App.useApp();
  const { formatDate } = useDateFormatter();
  const [loading, setLoading] = useState(false);
  const [tasks, setTasks] = useState<TodoItemData[]>([]);
  const [searchText, setSearchText] = useState("");
  const [searchField, setSearchField] = useState("Title");
  const [sortField, setSortField] = useState<string | undefined>(undefined);
  const [sortDirection, setSortDirection] = useState<boolean>(true);
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);

  const [open, setOpen] = useState(false);
  const [mode, setMode] = useState<Mode>("create");
  const [selectedId, setSelectedId] = useState<string | undefined>(undefined);
  const [submitting, setSubmitting] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [form] = Form.useForm<TodoItemResponse>();
  const [detailData, setDetailData] = useState<TodoItemResponse | null>(null);

  const toDayjs = (v: string | Dayjs | undefined): Dayjs | undefined => {
    if (!v) return undefined;
    return typeof v === "string" ? dayjs(v) : v;
  };

  const openCreate = () => {
    setMode("create");
    setSelectedId(undefined);
    setOpen(true);
  };

  const openDetail = (id?: string) => {
    if (!id) return;
    setMode("detail");
    setSelectedId(id);
    setOpen(true);
  };

  const openUpdate = (id?: string) => {
    if (!id) return;
    setMode("update");
    setSelectedId(id);
    setOpen(true);
  };

  const loadDetail = async (id: string) => {
    setDetailLoading(true);
    const response = await getTodoItemById(id);
    setDetailLoading(false);

    if (!response.isSuccess) {
      messageApi.error(response.message || "Failed to load task");
      return;
    }

    const raw = response.data as TodoItemResponse;
    const mapped: TodoItemResponse = {
      ...raw,
      dueDate: toDayjs(raw.dueDate) as dayjs.Dayjs,
      completedOn: toDayjs(raw.completedOn) as dayjs.Dayjs,
      createdOn: toDayjs(raw.createdOn),
    };

    setDetailData(mapped);

    if (mode === "update") {
      form.setFieldsValue({
        title: mapped.title,
        description: mapped.description,
        dueDate: mapped.dueDate,
        priority: mapped.priority,
        isCompleted: mapped.isCompleted,
        completedOn: mapped.completedOn,
      } as Partial<TodoItemResponse>);
    }
  };

  const onFinish = async (values: TodoItemResponse) => {
    try {
      setSubmitting(true);

      const formatDateOnly = (date: Dayjs | string | undefined): string | undefined => {
        if (!date) return undefined;
        const dayjsDate = typeof date === 'string' ? dayjs(date) : date;
        return dayjsDate.format('YYYY-MM-DD');
      };

      const common = {
        title: values.title,
        description: values.description,
        dueDate: formatDateOnly(values.dueDate),
        isCompleted: values.isCompleted || false,
        priority: values.priority,
        completedOn: formatDateOnly(values.completedOn),
      };

      if (mode === "create") {
        const payloadCreate: TodoItemRequest = {
          ...common,
        } as unknown as TodoItemRequest;

        const response = await createTodoItem(payloadCreate);
        if (!response.isSuccess) {
          messageApi.error(response.message || 'Tạo item thất bại');
        } else {
          messageApi.success(response.message || 'Tạo item thành công');
          setOpen(false);
          form.resetFields();
          
          // Reset về trang 1 và clear search để hiển thị tất cả items mới
          setSearchText('');
          if (currentPage === 1 && searchText === '') {
            // Nếu đã ở trang 1 và không có search, force reload
            await fetchTasks(1, pageSize, '');
          } else {
            // Chuyển về trang 1 sẽ tự động trigger useEffect
            setCurrentPage(1);
          }
        }
      } else if (mode === "update") {
        if (!selectedId) return messageApi.error('Thiếu ID item');

        const payloadUpdate = {
          ...common,
          id: selectedId,
        } as unknown as TodoItemRequest;

        const response = await updateTodoItem(payloadUpdate);
        if (!response.isSuccess) {
          messageApi.error(response.message || 'Cập nhật item thất bại');
        } else {
          messageApi.success(response.message || 'Cập nhật item thành công');
          setOpen(false);
          form.resetFields();
          // Reload trang hiện tại để cập nhật dữ liệu từ cache mới
          await fetchTasks(currentPage, pageSize, searchText);
        }
      }
    } catch (err) {
      console.log(err);
      messageApi.error('Có lỗi xảy ra');
    } finally {
      setSubmitting(false);
    }
  };

  const fetchTasks = useCallback(
    async (page: number, pageSizeArg: number, searchValue: string) => {
      try {
        setLoading(true);
        const filters: Filter[] = [];
        if (searchValue) {
          filters.push({
            fieldName: searchField,
            value: searchValue,
            operation: "Contains",
          });
        }

        const searchRequest: SearchRequest = {
          filters,
          sortBy: sortField
            ? {
                fieldName: sortField,
                ascending: sortDirection,
              }
            : undefined,
          pageIndex: page,
          pageSize: pageSizeArg,
        };

        const response = await searchTodoItems(searchRequest);
        if (response.isSuccess) {
          const searchResponse = response as unknown as SearchResponse<TodoItemData>;
          let taskData: TodoItemData[] = [];
          if (searchResponse.data && searchResponse.data.data) {
            taskData = searchResponse.data.data.map((item: TodoItemData | { data: TodoItemData }) =>
              "data" in item ? item.data : item
            );
          }

          setTasks(taskData);
          setTotal(searchResponse.data.totalRows);
        } else {
          messageApi.error(response.message || "Không thể tải danh sách items");
        }
      } catch (error) {
        console.error("Exception error:", error);
        messageApi.error("Không thể tải danh sách items");
      } finally {
        setLoading(false);
      }
    },
    [searchField, sortField, sortDirection]
  );

  useEffect(() => {
    fetchTasks(currentPage, pageSize, searchText);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchField, sortField, sortDirection, currentPage, pageSize, searchText]);

  const handleDelete = async (id: string) => {
    try {
      console.log('Deleting item with ID:', id);
      const response = await deleteTodoItem(id);
      console.log('Delete response:', response);
      
      if (response.isSuccess) {
        messageApi.success(response.message || 'Xóa item thành công');
        
        // Nếu xóa item cuối cùng của trang, quay về trang trước
        const newTotal = total - 1;
        const maxPage = Math.ceil(newTotal / pageSize);
        const targetPage = currentPage > maxPage ? Math.max(1, maxPage) : currentPage;
        
        if (targetPage !== currentPage) {
          setCurrentPage(targetPage);  // useEffect sẽ trigger
        } else {
          // Force re-fetch nếu vẫn ở cùng trang
          fetchTasks(targetPage, pageSize, searchText);
        }
      } else {
        console.error('Delete failed:', response);
        messageApi.error(response.message || 'Xóa item thất bại');
      }
    } catch (error) {
      console.error('Delete error:', error);
      messageApi.error('Có lỗi xảy ra khi xóa item');
    }
  };

  const handleSearch = () => {
    setCurrentPage(1);  // useEffect sẽ tự động trigger với searchText mới
  };

  const handleSortChange = (field: string) => {
    if (sortField === field) {
      setSortDirection(!sortDirection);
    } else {
      setSortField(field);
      setSortDirection(true);
    }
  };

  const renderSortIcon = (field: string) =>
    sortField === field ? (
      sortDirection ? (
        <SortAscendingOutlined />
      ) : (
        <SortDescendingOutlined />
      )
    ) : null;

  const columns = [
    {
      title: (
        <div className="column-header" onClick={() => handleSortChange("Title")}>
          Tiêu đề {renderSortIcon("Title")}
        </div>
      ),
      dataIndex: 'title',
      key: 'title',
      ellipsis: {
        showTitle: false,
      },
      render: (text: string, r: TodoItemData) => (
        <Space>
          {r.priority === Tier.High && <ExclamationCircleOutlined style={{ color: 'red' }} />}
          <Text strong ellipsis={{ tooltip: text }} style={{ maxWidth: 150 }}>
            {text}
          </Text>
        </Space>
      ),
    },
    {
      title: 'Mô tả',
      dataIndex: 'description',
      key: 'description',
      ellipsis: {
        showTitle: false,
      },
      render: (text: string) => (
        <Text ellipsis={{ tooltip: text }} style={{ maxWidth: 200 }}>
          {text || "-"}
        </Text>
      ),
    },
    {
      title: (
        <div className="column-header" onClick={() => handleSortChange("Priority")}>
          Độ ưu tiên {renderSortIcon("Priority")}
        </div>
      ),
      dataIndex: 'priority',
      key: 'priority',
      render: (priority: Tier) => <PriorityTag priority={priority} />,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isCompleted',
      key: 'isCompleted',
      render: (isCompleted: boolean) => <StatusTag isCompleted={isCompleted} />,
    },
    {
      title: (
        <div className="column-header" onClick={() => handleSortChange("DueDate")}>
          Hạn hoàn thành {renderSortIcon("DueDate")}
        </div>
      ),
      dataIndex: 'dueDate',
      key: 'dueDate',
      render: (date: string) => formatDate(date),
    },
    {
      title: 'Ngày hoàn thành',
      dataIndex: 'completedOn',
      key: 'completedOn',
      render: (date?: string) => formatDate(date),
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 80,
      align: 'center' as const,
      render: (_: unknown, record: TodoItemData) => {
        const menuItems: MenuProps['items'] = [
          {
            key: 'detail',
            label: 'Chi tiết',
            icon: <EyeOutlined />,
            onClick: () => openDetail(record.id),
          },
          {
            key: 'edit',
            label: 'Chỉnh sửa',
            icon: <EditOutlined />,
            onClick: () => openUpdate(record.id),
          },
          {
            type: 'divider',
          },
          {
            key: 'delete',
            label: 'Xóa',
            icon: <DeleteOutlined />,
            danger: true,
            onClick: () => {
              modal.confirm({
                title: 'Xác nhận xóa',
                content: 'Bạn có chắc chắn muốn xóa công việc này?',
                okText: 'Xóa',
                okType: 'danger',
                cancelText: 'Hủy',
                onOk: async () => {
                  if (!record.id) return;
                  await handleDelete(record.id);
                },
              });
            },
          },
        ];

        return (
          <Dropdown menu={{ items: menuItems }} trigger={['click']} placement="bottomRight">
            <Button type="text" icon={<MoreOutlined />} size="small" />
          </Dropdown>
        );
      },
    },
  ];

  const completedTasks = tasks.filter((t) => t.isCompleted).length;
  const highPriorityTasks = tasks.filter((t) => t.priority === Tier.High && !t.isCompleted).length;

  return (
    <div className="tasks-container">
      <div className="tasks-header">
        <h2>Quản lý công việc</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>
          Thêm công việc
        </Button>
      </div>

      <Row gutter={[16, 16]} style={{ marginBottom: 24 }}>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Tổng số công việc"
              value={total}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Đã hoàn thành (trang này)"
              value={completedTasks}
              suffix={`/ ${tasks.length}`}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col xs={24} sm={8}>
          <Card>
            <Statistic
              title="Ưu tiên cao (trang này)"
              value={highPriorityTasks}
              suffix={`/ ${tasks.length}`}
              prefix={<ExclamationCircleOutlined />}
              valueStyle={{ color: '#ff4d4f' }}
            />
          </Card>
        </Col>
      </Row>

      <div className="tasks-search">
        <div className="search-container">
          <Select
            defaultValue="Title"
            style={{ width: 120 }}
            onChange={(value) => setSearchField(value)}
            value={searchField}
          >
            <Option value="Title">Tiêu đề</Option>
            <Option value="Description">Mô tả</Option>
          </Select>
          <Input
            placeholder={`Tìm kiếm theo ${searchField === "Title" ? "tiêu đề" : "mô tả"}`}
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
            onPressEnter={handleSearch}
            suffix={<SearchOutlined onClick={handleSearch} />}
            style={{ width: 300 }}
          />
          <Button type="primary" onClick={handleSearch}>
            Tìm kiếm
          </Button>
        </div>
      </div>

      <Card>
        <Table
          className="tasks-table"
          columns={columns}
          dataSource={tasks}
          rowKey={(record) => record.id}
          loading={loading}
          scroll={{ x: 900 }}
          pagination={{
            current: currentPage,
            pageSize,
            total,
            showSizeChanger: true,
            showTotal: (t) => `Tổng ${t} công việc`,
            pageSizeOptions: ['10', '20', '50'],
            responsive: true,
            onChange: (page, size) => {
              setCurrentPage(page);
              setPageSize(size);
            },
          }}
        />
      </Card>

      <Modal
        title={
          mode === "create"
            ? "Thêm công việc mới"
            : mode === "update"
            ? "Cập nhật công việc"
            : "Chi tiết công việc"
        }
        open={open}
        onCancel={() => setOpen(false)}
        footer={
          mode === "detail"
            ? [
                <Button key="close" onClick={() => setOpen(false)}>
                  Đóng
                </Button>,
              ]
            : [
                <Button key="cancel" onClick={() => setOpen(false)}>
                  Hủy
                </Button>,
                <Button
                  key="submit"
                  type="primary"
                  loading={submitting}
                  onClick={() => form.submit()}
                >
                  {mode === "create" ? "Thêm mới" : "Cập nhật"}
                </Button>,
              ]
        }
        destroyOnHidden
        width={600}
        afterOpenChange={async (visible) => {
          if (!visible) {
            // Reset state khi đóng modal để tránh hiển thị data cũ
            setDetailData(null);
            form.resetFields();
            return;
          }

          if (mode !== "detail") {
            form.resetFields();
            if (mode === "create") {
              form.setFieldsValue({
                isCompleted: false,
                priority: Tier.Medium,
              });
            }
          }

          if ((mode === "detail" || mode === "update") && selectedId) {
            // Reset data cũ trước khi load mới
            setDetailData(null);
            await loadDetail(selectedId);
          }
        }}
      >
        {mode === "detail" ? (
          <Spin spinning={detailLoading}>
            <Descriptions column={1} bordered size="middle">
              <Descriptions.Item label="Tiêu đề">{detailData?.title ?? "-"}</Descriptions.Item>
              <Descriptions.Item label="Mô tả">{detailData?.description ?? "-"}</Descriptions.Item>
              <Descriptions.Item label="Độ ưu tiên">
                {detailData?.priority !== undefined ? <PriorityTag priority={detailData.priority} /> : "-"}
              </Descriptions.Item>
              <Descriptions.Item label="Trạng thái">
                {detailData?.isCompleted !== undefined ? <StatusTag isCompleted={detailData.isCompleted} /> : "-"}
              </Descriptions.Item>
              <Descriptions.Item label="Hạn hoàn thành">
                {formatDate(detailData?.dueDate as Dayjs | string)}
              </Descriptions.Item>
              <Descriptions.Item label="Ngày hoàn thành">
                {formatDate(detailData?.completedOn as Dayjs | string)}
              </Descriptions.Item>
              <Descriptions.Item label="Ngày tạo">
                {formatDate(detailData?.createdOn as Dayjs | string, true)}
              </Descriptions.Item>
              <Descriptions.Item label="Ngày chỉnh sửa">
                {formatDate(detailData?.modifiedOn as Dayjs | string, true)}
              </Descriptions.Item>
              {/* <Descriptions.Item label="Người tạo">
                {detailData?.createdBy ?? "-"}
              </Descriptions.Item> */}
            </Descriptions>
          </Spin>
        ) : (
          <Form form={form} layout="vertical" onFinish={onFinish} preserve={false}>
            <Form.Item
              label="Tiêu đề"
              name="title"
              rules={[{ required: true, message: 'Vui lòng nhập tiêu đề!' }]}
            >
              <Input placeholder="Nhập tiêu đề công việc" />
            </Form.Item>

            <Form.Item
              label="Mô tả"
              name="description"
              rules={[{ required: true, message: 'Vui lòng nhập mô tả!' }]}
            >
              <TextArea rows={4} placeholder="Nhập mô tả chi tiết" />
            </Form.Item>

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  label="Độ ưu tiên"
                  name="priority"
                  rules={[{ required: true, message: 'Vui lòng chọn độ ưu tiên!' }]}
                >
                  <Select placeholder="Chọn độ ưu tiên">
                    <Option value={Tier.Low}>Thấp</Option>
                    <Option value={Tier.Medium}>Trung bình</Option>
                    <Option value={Tier.High}>Cao</Option>
                  </Select>
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  label="Hạn hoàn thành"
                  name="dueDate"
                  rules={[{ required: true, message: 'Vui lòng chọn hạn hoàn thành!' }]}
                >
                  <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
                </Form.Item>
              </Col>
            </Row>

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item label="Đã hoàn thành" name="isCompleted" valuePropName="checked">
                  <Switch />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item
                  noStyle
                  shouldUpdate={(prevValues, currentValues) =>
                    prevValues.isCompleted !== currentValues.isCompleted
                  }
                >
                  {({ getFieldValue }) =>
                    getFieldValue('isCompleted') ? (
                      <Form.Item label="Ngày hoàn thành" name="completedOn">
                        <DatePicker style={{ width: '100%' }} format="DD/MM/YYYY" />
                      </Form.Item>
                    ) : null
                  }
                </Form.Item>
              </Col>
            </Row>
          </Form>
        )}
      </Modal>
    </div>
  );
};

export default TodoItems;
