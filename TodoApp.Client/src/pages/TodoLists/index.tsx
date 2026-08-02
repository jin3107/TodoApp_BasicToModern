import { useEffect, useState, useCallback } from "react";
import { ConfigProvider, App as AntdApp, Button, Input } from "antd";
import {
  PlusOutlined,
  FolderOutlined,
  EditOutlined,
  DeleteOutlined,
  SearchOutlined,
  SunOutlined,
  MoonOutlined,
  CheckSquareOutlined,
  UndoOutlined,
} from "@ant-design/icons";
import type { TodoListRequest, SearchRequest, SearchResponse } from "../../interfaces";
import {
  searchTodoLists,
  createTodoList,
  updateTodoList,
  deleteTodoList,
} from "../../apis/todoListAPI";
import TodoItems from "../TodoItems";
import { getClassicalTheme } from "./classicalTheme";
import "./style.scss";

const LISTS_PAGE_SIZE = 200;

interface TodoListData {
  id: string;
  name: string;
  description?: string;
  totalItems: number;
  completedItems: number;
}

interface ListDraft {
  name: string;
  description: string;
}

interface ListInlineFormProps {
  draft: ListDraft;
  onChange: (draft: ListDraft) => void;
  onCancel: () => void;
  onSave: () => void;
  submitting: boolean;
  autoFocus?: boolean;
}

const ListInlineForm = ({ draft, onChange, onCancel, onSave, submitting, autoFocus }: ListInlineFormProps) => (
  <div className="cl-inline-form">
    <Input
      placeholder="Tên danh sách"
      value={draft.name}
      autoFocus={autoFocus}
      onChange={(e) => onChange({ ...draft, name: e.target.value })}
      onPressEnter={onSave}
    />
    <Input.TextArea
      placeholder="Mô tả (không bắt buộc)"
      rows={2}
      value={draft.description}
      onChange={(e) => onChange({ ...draft, description: e.target.value })}
    />
    <div className="cl-inline-form-actions">
      <Button onClick={onCancel}>Hủy</Button>
      <Button type="primary" loading={submitting} onClick={onSave}>
        Lưu
      </Button>
    </div>
  </div>
);

const TodoListsPanel = ({
  theme,
  onToggleTheme,
}: {
  theme: "light" | "dark";
  onToggleTheme: () => void;
}) => {
  const { modal, message: messageApi, notification } = AntdApp.useApp();
  const [loading, setLoading] = useState(false);
  const [todoLists, setTodoLists] = useState<TodoListData[]>([]);
  const [searchInput, setSearchInput] = useState("");
  const [searchText, setSearchText] = useState("");
  const [selectedListId, setSelectedListId] = useState<string | undefined>();
  const [editingListId, setEditingListId] = useState<string | "new" | null>(null);
  const [listDraft, setListDraft] = useState<ListDraft>({ name: "", description: "" });
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    const t = setTimeout(() => setSearchText(searchInput.trim()), 300);
    return () => clearTimeout(t);
  }, [searchInput]);

  const loadTodoLists = useCallback(
    async (search: string) => {
      try {
        setLoading(true);
        const filters = search
          ? [{ fieldName: "Name", value: search, operation: "Contains" }]
          : [];
        const searchRequest: SearchRequest = { filters, pageIndex: 1, pageSize: LISTS_PAGE_SIZE };
        const response = await searchTodoLists(searchRequest);
        if (response.isSuccess) {
          const searchResponse = response as unknown as SearchResponse<TodoListData>;
          let data: TodoListData[] = [];
          if (searchResponse.data?.data) {
            data = searchResponse.data.data.map((item: TodoListData | { data: TodoListData }) =>
              "data" in item ? item.data : item,
            );
          }
          setTodoLists(data);
        } else {
          messageApi.error(response.message || "Không thể tải danh sách");
        }
      } catch {
        messageApi.error("Có lỗi xảy ra khi tải danh sách");
      } finally {
        setLoading(false);
      }
    },
    [messageApi],
  );

  useEffect(() => {
    loadTodoLists(searchText);
  }, [loadTodoLists, searchText]);

  const selectedList = todoLists.find((l) => l.id === selectedListId);

  const startCreate = () => {
    setEditingListId("new");
    setListDraft({ name: "", description: "" });
  };

  const startEdit = (list: TodoListData) => {
    setEditingListId(list.id);
    setListDraft({ name: list.name, description: list.description || "" });
  };

  const cancelEdit = () => setEditingListId(null);

  const saveEdit = async () => {
    if (!listDraft.name.trim()) {
      messageApi.warning("Vui lòng nhập tên danh sách");
      return;
    }
    setSubmitting(true);
    try {
      const request: TodoListRequest = {
        name: listDraft.name.trim(),
        description: listDraft.description.trim() || undefined,
      };

      if (editingListId && editingListId !== "new") {
        request.id = editingListId;
        const result = await updateTodoList(request);
        if (!result.isSuccess) {
          messageApi.error(result.message || "Cập nhật danh sách thất bại");
          return;
        }
        messageApi.success(result.message || "Cập nhật danh sách thành công");
      } else {
        const result = await createTodoList(request);
        if (!result.isSuccess) {
          messageApi.error(result.message || "Tạo danh sách thất bại");
          return;
        }
        messageApi.success(result.message || "Tạo danh sách thành công");
        if (result.data?.id) setSelectedListId(result.data.id);
      }

      setEditingListId(null);
      await loadTodoLists(searchText);
    } finally {
      setSubmitting(false);
    }
  };

  const requestDelete = (list: TodoListData) => {
    modal.confirm({
      title: "Xoá danh sách",
      content: (
        <p style={{ margin: 0 }}>
          Bạn có chắc muốn xoá "<strong>{list.name}</strong>"? Thao tác này sẽ xoá vĩnh viễn{" "}
          {list.totalItems} công việc bên trong và không thể hoàn tác.
        </p>
      ),
      okText: "Xoá",
      okType: "danger",
      cancelText: "Hủy",
      onOk: async () => {
        const result = await deleteTodoList(list.id);
        if (!result.isSuccess) {
          messageApi.error(result.message || "Xoá danh sách thất bại");
          return;
        }

        if (selectedListId === list.id) setSelectedListId(undefined);
        await loadTodoLists(searchText);

        const key = `undo-list-${list.id}-${Date.now()}`;
        notification.open({
          key,
          message: `Đã xoá danh sách "${list.name}"`,
          description: "Công việc bên trong danh sách đã xoá không thể khôi phục.",
          duration: 6,
          placement: "bottomRight",
          className: "cl-toast-notification",
          btn: (
            <Button
              size="small"
              type="link"
              icon={<UndoOutlined />}
              onClick={async () => {
                notification.destroy(key);
                const recreate = await createTodoList({
                  name: list.name,
                  description: list.description,
                });
                if (recreate.isSuccess) {
                  messageApi.success("Đã khôi phục danh sách");
                  await loadTodoLists(searchText);
                } else {
                  messageApi.error(recreate.message || "Không thể khôi phục danh sách");
                }
              }}
            >
              Hoàn tác
            </Button>
          ),
        });
      },
    });
  };

  return (
    <div className="todo-classical" data-theme={theme}>
      <div className="cl-toolbar">
        <div className="cl-toolbar-brand">
          <CheckSquareOutlined />
          <span>Việc Cần Làm</span>
        </div>
        <Button
          className="btn-icon-28"
          onClick={onToggleTheme}
          icon={theme === "dark" ? <SunOutlined /> : <MoonOutlined />}
          aria-label="Chuyển giao diện sáng/tối"
        />
      </div>

      <div className="cl-grid">
        <div className="cl-panel">
          <div className="cl-panel-header">
            <h3>
              <FolderOutlined /> Danh sách
            </h3>
            <Button type="primary" icon={<PlusOutlined />} onClick={startCreate}>
              Thêm danh sách
            </Button>
          </div>

          <div className="cl-search">
            <SearchOutlined />
            <Input
              placeholder="Tìm danh sách..."
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              allowClear
            />
          </div>

          {editingListId === "new" && (
            <ListInlineForm
              draft={listDraft}
              onChange={setListDraft}
              onCancel={cancelEdit}
              onSave={saveEdit}
              submitting={submitting}
              autoFocus
            />
          )}

          {loading ? (
            <div className="cl-rows">
              {[0, 1, 2].map((i) => (
                <div key={i} className="cl-skeleton-block" style={{ height: 62 }} />
              ))}
            </div>
          ) : todoLists.length === 0 ? (
            <div className="cl-empty">
              <FolderOutlined style={{ fontSize: 30 }} />
              <p>
                {searchText
                  ? "Không tìm thấy danh sách phù hợp."
                  : "Chưa có danh sách nào. Tạo danh sách đầu tiên để bắt đầu."}
              </p>
            </div>
          ) : (
            <div className="cl-rows">
              {todoLists.map((list) => {
                if (editingListId === list.id) {
                  return (
                    <ListInlineForm
                      key={list.id}
                      draft={listDraft}
                      onChange={setListDraft}
                      onCancel={cancelEdit}
                      onSave={saveEdit}
                      submitting={submitting}
                      autoFocus
                    />
                  );
                }

                const pct = list.totalItems > 0 ? Math.round((list.completedItems / list.totalItems) * 100) : 0;

                return (
                  <div
                    key={list.id}
                    className={`cl-list-row ${selectedListId === list.id ? "selected" : ""}`}
                    onClick={() => setSelectedListId(list.id)}
                  >
                    <div className="cl-row-title">
                      <h5>{list.name}</h5>
                      <div className="cl-row-actions">
                        <Button
                          className="btn-icon-28"
                          type="text"
                          icon={<EditOutlined />}
                          aria-label="Sửa danh sách"
                          onClick={(e) => {
                            e.stopPropagation();
                            startEdit(list);
                          }}
                        />
                        <Button
                          className="btn-icon-28"
                          type="text"
                          icon={<DeleteOutlined />}
                          aria-label="Xoá danh sách"
                          onClick={(e) => {
                            e.stopPropagation();
                            requestDelete(list);
                          }}
                        />
                      </div>
                    </div>
                    {list.description && <p className="cl-row-desc text-muted">{list.description}</p>}
                    <div className="cl-progress">
                      <div className="cl-progress-track">
                        <div className="cl-progress-bar" style={{ width: `${pct}%` }} />
                      </div>
                      <span className="cl-progress-count">
                        {list.completedItems}/{list.totalItems}
                      </span>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>

        <div className="cl-items-column">
          {selectedList ? (
            <TodoItems
              key={selectedList.id}
              todoListId={selectedList.id}
              todoListName={selectedList.name}
              onItemsChange={() => loadTodoLists(searchText)}
            />
          ) : (
            <div className="cl-panel cl-no-selection">
              <FolderOutlined style={{ fontSize: 32 }} />
              <p>Chọn một danh sách bên trái để xem công việc.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

const TodoLists = () => {
  const [theme, setTheme] = useState<"light" | "dark">(() =>
    typeof window !== "undefined" && window.matchMedia("(prefers-color-scheme: dark)").matches
      ? "dark"
      : "light",
  );

  return (
    <ConfigProvider theme={getClassicalTheme(theme === "dark")}>
      <AntdApp>
        <TodoListsPanel
          theme={theme}
          onToggleTheme={() => setTheme((t) => (t === "dark" ? "light" : "dark"))}
        />
      </AntdApp>
    </ConfigProvider>
  );
};

export default TodoLists;
