import { useEffect, useCallback, useReducer } from "react";
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

interface TodoListsQueryState {
  currentPage: number;
  pageSize: number;
  total: number;
  searchText: string;
}

type TodoListsQueryAction =
  | { type: "setPage"; page: number }
  | { type: "setPageSize"; page: number; pageSize: number }
  | { type: "setTotal"; total: number }
  | { type: "setSearchText"; searchText: string }
  | { type: "resetSearch" };

const initialQueryState: TodoListsQueryState = {
  currentPage: 1,
  pageSize: 5,
  total: 0,
  searchText: "",
};

const queryReducer = (
  state: TodoListsQueryState,
  action: TodoListsQueryAction,
): TodoListsQueryState => {
  switch (action.type) {
    case "setPage":
      return { ...state, currentPage: action.page };
    case "setPageSize":
      return { ...state, currentPage: action.page, pageSize: action.pageSize };
    case "setTotal":
      return { ...state, total: action.total };
    case "setSearchText":
      return { ...state, searchText: action.searchText };
    case "resetSearch":
      return { ...state, currentPage: 1, searchText: "" };
    default:
      return state;
  }
};

interface TodoListsViewState {
  loading: boolean;
  todoLists: TodoListData[];
  selectedList?: TodoListData;
  open: boolean;
  editingId?: string;
  submitting: boolean;
  detailLoading: boolean;
}

type TodoListsViewAction =
  | { type: "setLoading"; loading: boolean }
  | { type: "setTodoLists"; todoLists: TodoListData[] }
  | { type: "selectList"; list: TodoListData }
  | { type: "clearSelection"; id: string }
  | { type: "openCreate" }
  | { type: "openEdit"; id: string }
  | { type: "closeModal" }
  | { type: "setSubmitting"; submitting: boolean }
  | { type: "setDetailLoading"; detailLoading: boolean };

const initialViewState: TodoListsViewState = {
  loading: false,
  todoLists: [],
  selectedList: undefined,
  open: false,
  editingId: undefined,
  submitting: false,
  detailLoading: false,
};

const viewReducer = (
  state: TodoListsViewState,
  action: TodoListsViewAction,
): TodoListsViewState => {
  switch (action.type) {
    case "setLoading":
      return { ...state, loading: action.loading };
    case "setTodoLists":
      return { ...state, todoLists: action.todoLists };
    case "selectList":
      return { ...state, selectedList: action.list };
    case "clearSelection":
      return state.selectedList?.id === action.id
        ? { ...state, selectedList: undefined }
        : state;
    case "openCreate":
      return { ...state, open: true, editingId: undefined };
    case "openEdit":
      return { ...state, open: true, editingId: action.id };
    case "closeModal":
      return { ...state, open: false };
    case "setSubmitting":
      return { ...state, submitting: action.submitting };
    case "setDetailLoading":
      return { ...state, detailLoading: action.detailLoading };
    default:
      return state;
  }
};

const TodoLists = () => {
  const { modal, message: messageApi } = App.useApp();
  const [query, dispatchQuery] = useReducer(queryReducer, initialQueryState);
  const [view, dispatchView] = useReducer(viewReducer, initialViewState);
  const [form] = Form.useForm<{ name: string; description?: string }>();

  const loadTodoLists = useCallback(async (page: number = 1, search: string = "") => {
    try {
      dispatchView({ type: "setLoading", loading: true });
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
        pageSize: query.pageSize,
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

        dispatchView({ type: "setTodoLists", todoLists: todoListData });
        dispatchQuery({ type: "setTotal", total: searchResponse.data.totalRows });
      } else {
        messageApi.error(response.message || "Không thể tải danh sách todos");
      }
    } catch {
      messageApi.error("An error occurred while loading todo lists");
    } finally {
      dispatchView({ type: "setLoading", loading: false });
    }
  }, [messageApi, query.pageSize]);

  useEffect(() => {
    loadTodoLists(query.currentPage, query.searchText);
  }, [loadTodoLists, query.currentPage, query.searchText]);

  const handleSelectList = (list: TodoListData) => {
    dispatchView({ type: "selectList", list });
  };

  const handleCreateList = () => {
    dispatchView({ type: "openCreate" });
  };

  const handleEditList = (list: TodoListData) => {
    dispatchView({ type: "openEdit", id: list.id });
  };

  const loadDetail = async (id: string) => {
    dispatchView({ type: "setDetailLoading", detailLoading: true });
    const response = await getTodoListById(id);
    dispatchView({ type: "setDetailLoading", detailLoading: false });

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
          <p className="danger-note">
            This will permanently delete ALL {list.totalItems} todo item(s) in this list!
          </p>
          <p className="muted-note">
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
            dispatchView({ type: "clearSelection", id: list.id });
            
            // Handle pagination after delete
            const newTotal = query.total - 1;
            const maxPage = Math.ceil(newTotal / query.pageSize);
            const targetPage = query.currentPage > maxPage ? Math.max(1, maxPage) : query.currentPage;
            
            if (targetPage !== query.currentPage) {
              dispatchQuery({ type: "setPage", page: targetPage }); // useEffect will trigger reload
            } else {
              // Force re-fetch if staying on same page
              loadTodoLists(targetPage, query.searchText);
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
      dispatchView({ type: "setSubmitting", submitting: true });

      const request: TodoListRequest = {
        name: values.name,
        description: values.description, // Always include, can be undefined
      };

      // Only add id for update requests
      if (view.editingId) {
        request.id = view.editingId;
      }

      if (view.editingId) {
        // Update existing todo list
        const result = await updateTodoList(request);
        if (result.isSuccess) {
          messageApi.success(result.message || "Todo list updated successfully");
          dispatchView({ type: "closeModal" });
          form.resetFields();
          // Reload current page to update data
          await loadTodoLists(query.currentPage, query.searchText);
        } else {
          messageApi.error(result.message || "Failed to update todo list");
        }
      } else {
        // Create new todo list
        const result = await createTodoList(request);
        if (result.isSuccess) {
          messageApi.success(result.message || "Todo list created successfully");
          dispatchView({ type: "closeModal" });
          form.resetFields();
          
          // Reset to page 1 and clear search to show all items including new one
          dispatchQuery({ type: "resetSearch" });
          if (query.currentPage === 1 && query.searchText === "") {
            // If already on page 1 and no search, force reload
            await loadTodoLists(1, '');
          }
        } else {
          messageApi.error(result.message || "Failed to create todo list");
        }
      }
    } catch (error) {
      console.error('Submit error:', error);
      messageApi.error("An error occurred while saving the todo list");
    } finally {
      dispatchView({ type: "setSubmitting", submitting: false });
    }
  };

  const onTodoItemsChange = () => {
    // Refresh current page to update progress counts
    loadTodoLists(query.currentPage, query.searchText);
  };

  const handleSearch = () => {
    dispatchQuery({ type: "setPage", page: 1 });
  };

  const handlePageChange = (page: number, size?: number) => {
    if (size) {
      dispatchQuery({ type: "setPageSize", page, pageSize: size });
      return;
    }

    dispatchQuery({ type: "setPage", page });
  };

  return (
    <div className="todo-lists-management page-shell">
      <Row gutter={[16, 16]} className="todo-lists-grid">
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
            <div className="todo-list-search">
              <Input.Search
                placeholder="Search todo lists..."
                value={query.searchText}
                onChange={(e) =>
                  dispatchQuery({
                    type: "setSearchText",
                    searchText: e.target.value,
                  })
                }
                onSearch={handleSearch}
              />
            </div>

            <Spin spinning={view.loading}>
              {view.todoLists.length === 0 ? (
                <Empty
                  description="No todo lists yet"
                  image={Empty.PRESENTED_IMAGE_SIMPLE}
                />
              ) : (
                <List
                  dataSource={view.todoLists}
                  renderItem={(list) => (
                    <List.Item
                      className={`todo-list-item ${
                        view.selectedList?.id === list.id ? "selected" : ""
                      }`}
                      onClick={() => handleSelectList(list)}
                      actions={[
                        <Button
                          key="edit"
                          type="text"
                          size="small"
                          icon={<EditOutlined />}
                          onClick={(e) => {
                            e.stopPropagation();
                            handleEditList(list);
                          }}
                        />,
                        <Button
                          key="delete"
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
              {query.total > 0 && (
                <div className="todo-list-pagination">
                  <Pagination
                    current={query.currentPage}
                    total={query.total}
                    pageSize={query.pageSize}
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
          {view.selectedList ? (
            <TodoItems
              key={view.selectedList.id} // Force re-render when todoListId changes
              todoListId={view.selectedList.id}
              todoListName={view.selectedList.name}
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
        title={`${view.editingId ? "Edit" : "Create"} Todo List`}
        open={view.open}
        onOk={handleSubmit}
        onCancel={() => dispatchView({ type: "closeModal" })}
        confirmLoading={view.submitting}
        destroyOnHidden
        afterOpenChange={async (isOpen) => {
          if (isOpen && view.editingId) {
            // Load detail data when editing
            await loadDetail(view.editingId);
          }
        }}
      >
        <Spin spinning={view.detailLoading}>
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
