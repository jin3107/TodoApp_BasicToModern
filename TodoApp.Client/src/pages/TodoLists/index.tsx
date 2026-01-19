import { useState, useEffect } from "react";
import {
  Row,
  Col,
  Card,
  List,
  Button,
  Modal,
  Form,
  Input,
  App,
  Typography,
  Progress,
  Empty,
  Space,
  Spin,
  Pagination,
} from "antd";
import {
  PlusOutlined,
  FolderOutlined,
  EditOutlined,
  DeleteOutlined,
} from "@ant-design/icons";
import type {
  TodoListRequest,
  SearchRequest,
  SearchResponse,
} from "../../interfaces";
import {
  searchTodoLists,
  createTodoList,
  updateTodoList,
  deleteTodoList,
  getTodoListById,
} from "../../apis/todoListAPI";
import TodoItems from "../TodoItems";
import "./style.scss";

const { TextArea } = Input;
const { Text } = Typography;

interface TodoListData {
  id: string;
  name: string;
  description?: string;
  totalItems: number;
  completedItems: number;
  createdOn?: string;
  modifiedOn?: string;
}

const TodoLists = () => {
  const { modal, message: messageApi } = App.useApp();
  const [loading, setLoading] = useState(false);
  const [todoLists, setTodoLists] = useState<TodoListData[]>([]);
  const [selectedListId, setSelectedListId] = useState<string | undefined>(
    undefined,
  );
  const [selectedList, setSelectedList] = useState<TodoListData | undefined>(
    undefined,
  );

  // Pagination states
  const [currentPage, setCurrentPage] = useState(1);
  const [pageSize, setPageSize] = useState(5);
  const [total, setTotal] = useState(0);
  const [searchText, setSearchText] = useState("");

  // Modal states
  const [open, setOpen] = useState(false);
  const [editingId, setEditingId] = useState<string | undefined>(undefined);
  const [submitting, setSubmitting] = useState(false);
  const [detailLoading, setDetailLoading] = useState(false);
  const [form] = Form.useForm<{ name: string; description?: string }>();

  const loadTodoLists = async (page: number = 1, search: string = "") => {
    try {
      setLoading(true);
      const filters = [];
      if (search) {
        filters.push({
          fieldName: "Name",
          value: search,
          operation: "Contains",
        });
      }

      const searchRequest: SearchRequest = {
        filters,
        pageIndex: page,
        pageSize,
      };

      const response = await searchTodoLists(searchRequest);
      if (response.isSuccess) {
        const searchResponse =
          response as unknown as SearchResponse<TodoListData>;
        let todoListData: TodoListData[] = [];
        if (searchResponse.data && searchResponse.data.data) {
          todoListData = searchResponse.data.data.map(
            (item: TodoListData | { data: TodoListData }) =>
              "data" in item ? item.data : item,
          );
        }

        setTodoLists(todoListData);
        setTotal(searchResponse.data.totalRows);
      } else {
        messageApi.error(response.message || "Không thể tải danh sách todos");
      }
    } catch (error) {
      messageApi.error("An error occurred while loading todo lists");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTodoLists(currentPage, searchText);
  }, [currentPage, pageSize, searchText]);

  const handleSelectList = (list: TodoListData) => {
    setSelectedListId(list.id);
    setSelectedList(list);
  };

  const handleCreateList = () => {
    setEditingId(undefined);
    setOpen(true);
  };

  const handleEditList = (list: TodoListData) => {
    setEditingId(list.id);
    setOpen(true);
  };

  const loadDetail = async (id: string) => {
    setDetailLoading(true);
    const response = await getTodoListById(id);
    setDetailLoading(false);

    if (!response.isSuccess) {
      messageApi.error(response.message || "Failed to load todo list");
      return;
    }

    const todoListData = response.data;
    form.setFieldsValue({
      name: todoListData.name,
      description: todoListData.description,
    });
  };

  const handleDeleteList = (list: TodoListData) => {
    modal.confirm({
      title: "Delete Todo List",
      content: (
        <div>
          <p>Are you sure you want to delete "<strong>{list.name}</strong>"?</p>
          <p style={{ color: '#ff4d4f', fontWeight: 'bold', marginTop: '8px' }}>
            ⚠️ This will permanently delete ALL {list.totalItems} todo item(s) in this list!
          </p>
          <p style={{ color: '#666', fontSize: '12px', marginTop: '8px' }}>
            This action cannot be undone.
          </p>
        </div>
      ),
      okText: "Delete",
      okType: "danger",
      width: 450,
      onOk: async () => {
        try {
          console.log('Deleting todo list with ID:', list.id);
          const result = await deleteTodoList(list.id);
          console.log('Delete response:', result);
          
          if (result.isSuccess) {
            messageApi.success(result.message || "Todo list deleted successfully");
            
            // Clear selection if deleted item was selected
            if (selectedListId === list.id) {
              setSelectedListId(undefined);
              setSelectedList(undefined);
            }
            
            // Handle pagination after delete
            const newTotal = total - 1;
            const maxPage = Math.ceil(newTotal / pageSize);
            const targetPage = currentPage > maxPage ? Math.max(1, maxPage) : currentPage;
            
            if (targetPage !== currentPage) {
              setCurrentPage(targetPage); // useEffect will trigger reload
            } else {
              // Force re-fetch if staying on same page
              loadTodoLists(targetPage, searchText);
            }
          } else {
            console.error('Delete failed:', result);
            messageApi.error(result.message || "Failed to delete todo list");
          }
        } catch (error) {
          console.error('Delete error:', error);
          messageApi.error("An error occurred while deleting the todo list");
        }
      },
    });
  };

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields();
      setSubmitting(true);

      const request: TodoListRequest = {
        name: values.name,
        description: values.description, // Always include, can be undefined
      };

      // Only add id for update requests
      if (editingId) {
        request.id = editingId;
      }

      if (editingId) {
        // Update existing todo list
        const result = await updateTodoList(request);
        if (result.isSuccess) {
          messageApi.success(result.message || "Todo list updated successfully");
          setOpen(false);
          form.resetFields();
          // Reload current page to update data
          await loadTodoLists(currentPage, searchText);
        } else {
          messageApi.error(result.message || "Failed to update todo list");
        }
      } else {
        // Create new todo list
        const result = await createTodoList(request);
        if (result.isSuccess) {
          messageApi.success(result.message || "Todo list created successfully");
          setOpen(false);
          form.resetFields();
          
          // Reset to page 1 and clear search to show all items including new one
          setSearchText('');
          if (currentPage === 1 && searchText === '') {
            // If already on page 1 and no search, force reload
            await loadTodoLists(1, '');
          } else {
            // Change to page 1 will trigger useEffect
            setCurrentPage(1);
          }
        } else {
          messageApi.error(result.message || "Failed to create todo list");
        }
      }
    } catch (error) {
      console.error('Submit error:', error);
      messageApi.error("An error occurred while saving the todo list");
    } finally {
      setSubmitting(false);
    }
  };

  const onTodoItemsChange = () => {
    // Refresh current page to update progress counts
    loadTodoLists(currentPage, searchText);
  };

  const handleSearch = () => {
    setCurrentPage(1);
  };

  const handlePageChange = (page: number, size?: number) => {
    setCurrentPage(page);
    if (size) setPageSize(size);
  };

  return (
    <div className="todo-lists-management">
      <Row gutter={[16, 16]} style={{ height: "calc(100vh - 64px)" }}>
        {/* Left Column - Todo Lists */}
        <Col xs={24} lg={8} className="todo-lists-column">
          <Card
            title={
              <Space>
                <FolderOutlined />
                <span>Todo Lists</span>
              </Space>
            }
            extra={
              <Button
                type="primary"
                icon={<PlusOutlined />}
                onClick={handleCreateList}
              >
                Add List
              </Button>
            }
            className="todo-lists-card"
          >
            {/* Search Box */}
            <div style={{ marginBottom: 16 }}>
              <Input.Search
                placeholder="Search todo lists..."
                value={searchText}
                onChange={(e) => setSearchText(e.target.value)}
                onSearch={handleSearch}
                style={{ marginBottom: 8 }}
              />
            </div>

            <Spin spinning={loading}>
              {todoLists.length === 0 ? (
                <Empty
                  description="No todo lists yet"
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                />
              ) : (
                <List
                  dataSource={todoLists}
                  renderItem={(list) => (
                    <List.Item
                      className={`todo-list-item ${
                        selectedListId === list.id ? "selected" : ""
                      }`}
                      onClick={() => handleSelectList(list)}
                      actions={[
                        <Button
                          type="text"
                          size="small"
                          icon={<EditOutlined />}
                          onClick={(e) => {
                            e.stopPropagation();
                            handleEditList(list);
                          }}
                        />,
                        <Button
                          type="text"
                          size="small"
                          danger
                          icon={<DeleteOutlined />}
                          onClick={(e) => {
                            e.stopPropagation();
                            handleDeleteList(list);
                          }}
                        />,
                      ]}
                    >
                      <List.Item.Meta
                        title={list.name}
                        description={
                          <Space direction="vertical" size="small">
                            {list.description && (
                              <Text type="secondary">{list.description}</Text>
                            )}
                            <Progress
                              percent={
                                list.totalItems > 0
                                  ? Math.round(
                                      (list.completedItems / list.totalItems) *
                                        100,
                                    )
                                  : 0
                              }
                              size="small"
                              format={() =>
                                `${list.completedItems}/${list.totalItems}`
                              }
                            />
                          </Space>
                        }
                      />
                    </List.Item>
                  )}
                />
              )}

              {/* Pagination */}
              {total > 0 && (
                <div style={{ marginTop: 16, textAlign: "center" }}>
                  <Pagination
                    current={currentPage}
                    total={total}
                    pageSize={pageSize}
                    showSizeChanger
                    showTotal={(t, range) =>
                      `${range[0]}-${range[1]} of ${t} todo lists`
                    }
                    pageSizeOptions={["5", "10", "20"]}
                    responsive
                    onChange={handlePageChange}
                  />
                </div>
              )}
            </Spin>
          </Card>
        </Col>

        {/* Right Column - Todo Items */}
        <Col xs={24} lg={16} className="todo-items-column">
          {selectedListId && selectedList ? (
            <TodoItems
              key={selectedListId} // Force re-render when todoListId changes
              todoListId={selectedListId}
              todoListName={selectedList.name}
              onItemsChange={onTodoItemsChange}
            />
          ) : (
            <Card className="empty-selection">
              <Empty
                description="Select a todo list to view its items"
                image={Empty.PRESENTED_IMAGE_SIMPLE}
              />
            </Card>
          )}
        </Col>
      </Row>

      {/* Create/Edit Modal */}
      <Modal
        title={`${editingId ? "Edit" : "Create"} Todo List`}
        open={open}
        onOk={handleSubmit}
        onCancel={() => setOpen(false)}
        confirmLoading={submitting}
        destroyOnHidden
        afterOpenChange={async (isOpen) => {
          if (isOpen && editingId) {
            // Load detail data when editing
            await loadDetail(editingId);
          }
        }}
      >
        <Spin spinning={detailLoading}>
          <Form form={form} layout="vertical" preserve={false}>
            <Form.Item
              label="Name"
              name="name"
              rules={[
                { required: true, message: "Please enter a name" },
                { max: 100, message: "Name must be less than 100 characters" },
              ]}
            >
              <Input placeholder="Enter todo list name" />
            </Form.Item>
            <Form.Item
              label="Description"
              name="description"
              rules={[
                {
                  max: 500,
                  message: "Description must be less than 500 characters",
                },
              ]}
            >
              <TextArea rows={3} placeholder="Enter description (optional)" />
            </Form.Item>
          </Form>
        </Spin>
      </Modal>
    </div>
  );
};

export default TodoLists;
