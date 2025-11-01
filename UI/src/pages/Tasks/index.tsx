import { useState, useEffect, useCallback } from "react";
import {
  Button,
  Input,
  Table,
  message,
  Select,
  Modal,
  Form,
  Popconfirm,
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
} from "antd";
import {
  PlusOutlined,
  CheckCircleOutlined,
  ClockCircleOutlined,
  ExclamationCircleOutlined,
  SearchOutlined,
  SortAscendingOutlined,
  SortDescendingOutlined,
} from "@ant-design/icons";
import dayjs, { Dayjs } from "dayjs";
import { Tier } from "../../commons";
import type { Filter, SearchRequest, SearchResponse, TaskRequest, TaskResponse } from "../../interfaces";
import { createTask, deleteTask, getTaskById, searchTasks, updateTask } from "../../apis/taskAPI";
import "./style.scss";

const { TextArea } = Input;
const { Option } = Select;
const { Text } = Typography;

type Mode = "create" | "detail" | "update";

interface TaskData {
  id: string;
  title: string;
  description: string;
  dueDate: string;
  isCompleted: boolean;
  priority: Tier;
  completedOn?: string;
}

const Tasks = () => {
  const [loading, setLoading] = useState(false);
  const [tasks, setTasks] = useState<TaskData[]>([]);
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
  const [form] = Form.useForm<TaskResponse>();
  const [detailData, setDetailData] = useState<TaskResponse | null>(null);

  const toDayjs = (v: string | Dayjs | undefined): Dayjs | undefined => {
    if (!v) return undefined;
    return typeof v === "string" ? dayjs(v) : v;
  };

  const fmtDate = (v?: string | Dayjs, withTime = false) => {
    if (!v) return "-";
    const date = typeof v === "string" ? dayjs(v) : v;
    return date ? date.format(withTime ? "DD/MM/YYYY HH:mm" : "DD/MM/YYYY") : "-";
  };

  const getPriorityTag = (priority: Tier) => {
    switch (priority) {
      case Tier.High:
        return <Tag color="red">Cao</Tag>;
      case Tier.Medium:
        return <Tag color="orange">Trung bình</Tag>;
      case Tier.Low:
        return <Tag color="green">Thấp</Tag>;
      default:
        return <Tag>Không xác định</Tag>;
    }
  };

  const getStatusTag = (isCompleted: boolean) => {
    return isCompleted ? (
      <Tag icon={<CheckCircleOutlined />} color="success">Hoàn thành</Tag>
    ) : (
      <Tag icon={<ClockCircleOutlined />} color="processing">Chưa hoàn thành</Tag>
    );
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
    const response = await getTaskById(id);
    setDetailLoading(false);

    if (!response.isSuccess) {
      message.error(response.message || "Failed to load task");
      return;
    }

    const raw = response.data as TaskResponse;
    const mapped: TaskResponse = {
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
      } as Partial<TaskResponse>);
    }
  };

  const onFinish = async (values: TaskResponse) => {
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
        const payloadCreate: TaskRequest = {
          ...common,
        } as unknown as TaskRequest;

        const response = await createTask(payloadCreate);
        if (!response.isSuccess) {
          message.error(response.message || 'Tạo task thất bại');
        } else {
          message.success(response.message || 'Tạo task thành công');
          setOpen(false);
          fetchTasks(currentPage, pageSize, searchText);
        }
      } else if (mode === "update") {
        if (!selectedId) return message.error('Thiếu ID task');

        const payloadUpdate = {
          ...common,
          id: selectedId,
        } as unknown as TaskRequest;

        const response = await updateTask(payloadUpdate);
        if (!response.isSuccess) {
          message.error(response.message || 'Cập nhật task thất bại');
        } else {
          message.success(response.message || 'Cập nhật task thành công');
          setOpen(false);
          fetchTasks(currentPage, pageSize, searchText);
        }
      }
    } catch (err) {
      console.log(err);
      message.error('Có lỗi xảy ra');
    } finally {
      setSubmitting(false);
    }
  };

  const fetchTasks = useCallback(
    async (page = 1, pageSizeArg = 10, searchValue = "") => {
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

        const response = await searchTasks(searchRequest);
        if (response.isSuccess) {
          const searchResponse = response as unknown as SearchResponse<TaskData>;
          let taskData: TaskData[] = [];
          if (searchResponse.data && searchResponse.data.data) {
            taskData = searchResponse.data.data.map((item: TaskData | { data: TaskData }) =>
              "data" in item ? item.data : item
            );
          }

          setTasks(taskData);
          setCurrentPage(searchResponse.data.currentPage);
          setPageSize(searchResponse.data.rowsPerPage);
          setTotal(searchResponse.data.totalRows);
        } else {
          message.error(response.message || "Không thể tải danh sách tasks");
        }
      } catch (error) {
        console.error("Exception error:", error);
        message.error("Không thể tải danh sách tasks");
      } finally {
        setLoading(false);
      }
    },
    [searchField, sortField, sortDirection]
  );

  useEffect(() => {
    fetchTasks(currentPage, pageSize, searchText);
  }, [fetchTasks, currentPage, pageSize, searchText]);

  const handleDelete = async (id: string) => {
    try {
      const response = await deleteTask(id);
      if (response.isSuccess) {
        message.success(response.message || 'Xóa task thành công');
        fetchTasks(currentPage, pageSize, searchText);
      } else {
        message.error(response.message || 'Xóa task thất bại');
      }
    } catch (error) {
      message.error('Có lỗi xảy ra khi xóa task');
      console.error(error);
    }
  };

  const handleSearch = () => {
    fetchTasks(1, pageSize, searchText);
  };

  const handleSortChange = (field: string) => {
    if (sortField === field) {
      setSortDirection(!sortDirection);
    } else {
      setSortField(field);
      setSortDirection(true);
    }
    fetchTasks(currentPage, pageSize, searchText);
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
      width: '10%',
      render: (text: string, r: TaskData) => (
        <Space>
          {r.priority === Tier.High && <ExclamationCircleOutlined style={{ color: 'red' }} />}
          <Text strong ellipsis={{ tooltip: text }}>{text}</Text>
        </Space>
      ),
    },
    {
      title: 'Mô tả',
      dataIndex: 'description',
      key: 'description',
      width: '20%',
      ellipsis: true,
      render: (text: string) => (
        <Text ellipsis={{ tooltip: text }}>{text || "-"}</Text>
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
      width: '10%',
      render: (priority: Tier) => getPriorityTag(priority),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isCompleted',
      key: 'isCompleted',
      width: '12%',
      render: (isCompleted: boolean) => getStatusTag(isCompleted),
    },
    {
      title: (
        <div className="column-header" onClick={() => handleSortChange("DueDate")}>
          Hạn hoàn thành {renderSortIcon("DueDate")}
        </div>
      ),
      dataIndex: 'dueDate',
      key: 'dueDate',
      width: '13%',
      render: (date: string) => fmtDate(date, false),
    },
    {
      title: 'Ngày hoàn thành',
      dataIndex: 'completedOn',
      key: 'completedOn',
      width: '13%',
      render: (date?: string) => fmtDate(date, false),
    },
    {
      title: 'Thao tác',
      key: 'action',
      fixed: 'right' as const,
      width: 200,
      render: (_: unknown, record: TaskData) => (
        <div className="action-buttons" style={{ whiteSpace: "nowrap" }}>
          <Button type="link" onClick={() => openDetail(record.id)}>
            Chi tiết
          </Button>
          <Button type="link" onClick={() => openUpdate(record.id)}>
            Sửa
          </Button>
          <Popconfirm
            title="Xóa task này?"
            okText="Xóa"
            cancelText="Hủy"
            onConfirm={async () => {
              if (!record.id) return;
              await handleDelete(record.id);
            }}
          >
            <Button type="link" danger>
              Xóa
            </Button>
          </Popconfirm>
        </div>
      ),
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

      <Row gutter={16} style={{ marginBottom: 24 }}>
        <Col span={8}>
          <Card>
            <Statistic
              title="Tổng số công việc"
              value={tasks.length}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#1890ff' }}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Statistic
              title="Đã hoàn thành"
              value={completedTasks}
              prefix={<CheckCircleOutlined />}
              valueStyle={{ color: '#52c41a' }}
            />
          </Card>
        </Col>
        <Col span={8}>
          <Card>
            <Statistic
              title="Ưu tiên cao"
              value={highPriorityTasks}
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
          pagination={{
            current: currentPage,
            pageSize,
            total,
            showSizeChanger: true,
            showTotal: (t) => `Tổng ${t} công việc`,
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
          if (!visible) return;

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
                {detailData?.priority !== undefined ? getPriorityTag(detailData.priority) : "-"}
              </Descriptions.Item>
              <Descriptions.Item label="Trạng thái">
                {detailData?.isCompleted !== undefined ? getStatusTag(detailData.isCompleted) : "-"}
              </Descriptions.Item>
              <Descriptions.Item label="Hạn hoàn thành">
                {fmtDate(detailData?.dueDate as Dayjs | string, false)}
              </Descriptions.Item>
              <Descriptions.Item label="Ngày hoàn thành">
                {fmtDate(detailData?.completedOn as Dayjs | string, false)}
              </Descriptions.Item>
              <Descriptions.Item label="Ngày tạo">
                {fmtDate(detailData?.createdOn as Dayjs | string, true)}
              </Descriptions.Item>
              <Descriptions.Item label="Ngày chỉnh sửa">
                {fmtDate(detailData?.modifiedOn as Dayjs | string, true)}
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

export default Tasks;
