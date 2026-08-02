import { useState, useEffect, useCallback, useMemo } from "react";
import { Button, Input, Table, Select, Popover, Checkbox, App } from "antd";
import {
  PlusOutlined,
  SearchOutlined,
  SortAscendingOutlined,
  SortDescendingOutlined,
  EditOutlined,
  DeleteOutlined,
  CalendarOutlined,
  UndoOutlined,
} from "@ant-design/icons";
import dayjs from "dayjs";
import { Tier } from "../../commons/enums/Tier";
import type { Filter, SearchRequest, SearchResponse, TodoItemRequest } from "../../interfaces";
import {
  createTodoItem,
  deleteTodoItem,
  searchTodoItems,
  updateTodoItem,
} from "../../apis/todoItemAPI";
import { PriorityTag } from "../../components/PriorityTag";
import { StatusTag } from "../../components/StatusTag";
import { useDateFormatter } from "../../components/useDateFormatter";
import { useIsDesktop } from "../../components/useIsDesktop";
import "./style.scss";

const ITEMS_PAGE_SIZE = 500;
const NEW_ROW_ID = "__new_item__";

type SortField = "title" | "priority" | "dueDate";

interface TodoItemData {
  id: string;
  title: string;
  description: string;
  dueDate: string;
  isCompleted: boolean;
  priority: Tier;
  completedOn?: string;
}

interface ItemDraft {
  title: string;
  description: string;
  priority: Tier;
  dueDate: string;
}

interface TodoItemsProps {
  todoListId: string;
  todoListName: string;
  onItemsChange: () => void;
}

interface SortHeaderProps {
  label: string;
  field: SortField;
  sortField?: SortField;
  sortAsc: boolean;
  onSort: (field: SortField) => void;
}

const SortHeader = ({ label, field, sortField, sortAsc, onSort }: SortHeaderProps) => (
  <button
    type="button"
    className="column-header"
    aria-label={`Sắp xếp theo ${label}`}
    aria-pressed={sortField === field}
    onClick={() => onSort(field)}
  >
    <span>{label}</span>
    {sortField === field && (sortAsc ? <SortAscendingOutlined /> : <SortDescendingOutlined />)}
  </button>
);

const isOverdue = (dueDate: string | undefined, isCompleted: boolean) => {
  if (!dueDate || isCompleted) return false;
  return dayjs(dueDate).endOf("day").isBefore(dayjs());
};

const emptyDraft: ItemDraft = { title: "", description: "", priority: Tier.Medium, dueDate: "" };

const ItemInlineForm = ({
  draft,
  onChange,
  onCancel,
  onSave,
  submitting,
  autoFocus,
}: {
  draft: ItemDraft;
  onChange: (d: ItemDraft) => void;
  onCancel: () => void;
  onSave: () => void;
  submitting: boolean;
  autoFocus?: boolean;
}) => (
  <div className="cl-inline-form">
    <Input
      placeholder="Tiêu đề"
      value={draft.title}
      autoFocus={autoFocus}
      onChange={(e) => onChange({ ...draft, title: e.target.value })}
    />
    <Input
      placeholder="Mô tả"
      value={draft.description}
      onChange={(e) => onChange({ ...draft, description: e.target.value })}
    />
    <div className="cl-inline-form-row">
      <Select
        value={draft.priority}
        style={{ flex: 1 }}
        onChange={(value) => onChange({ ...draft, priority: value })}
        options={[
          { value: Tier.Low, label: "Thấp" },
          { value: Tier.Medium, label: "Trung bình" },
          { value: Tier.High, label: "Cao" },
        ]}
      />
      <input
        type="date"
        className="cl-date-input"
        style={{ flex: 1 }}
        value={draft.dueDate}
        onChange={(e) => onChange({ ...draft, dueDate: e.target.value })}
      />
    </div>
    <div className="cl-inline-form-actions">
      <Button onClick={onCancel}>Hủy</Button>
      <Button type="primary" loading={submitting} onClick={onSave}>
        Lưu
      </Button>
    </div>
  </div>
);

const TodoItems = ({ todoListId, todoListName, onItemsChange }: TodoItemsProps) => {
  const { message: messageApi, notification } = App.useApp();
  const { formatDate } = useDateFormatter();
  const isDesktop = useIsDesktop(1024);

  const [loading, setLoading] = useState(false);
  const [items, setItems] = useState<TodoItemData[]>([]);
  const [searchInput, setSearchInput] = useState("");
  const [searchText, setSearchText] = useState("");
  const [searchField, setSearchField] = useState<"title" | "description">("title");
  const [sortField, setSortField] = useState<SortField | undefined>(undefined);
  const [sortAsc, setSortAsc] = useState(true);
  const [showMoreColumns, setShowMoreColumns] = useState(false);

  const [editingItemId, setEditingItemId] = useState<string | "new" | null>(null);
  const [itemDraft, setItemDraft] = useState<ItemDraft>(emptyDraft);
  const [submitting, setSubmitting] = useState(false);
  const [openPriorityId, setOpenPriorityId] = useState<string | null>(null);
  const [openDateId, setOpenDateId] = useState<string | null>(null);

  useEffect(() => {
    const t = setTimeout(() => setSearchText(searchInput.trim()), 300);
    return () => clearTimeout(t);
  }, [searchInput]);

  const fetchItems = useCallback(async () => {
    try {
      setLoading(true);
      const filters: Filter[] = [
        { fieldName: "TodoListId", value: todoListId, operation: "Equals" },
      ];
      const searchRequest: SearchRequest = {
        filters,
        pageIndex: 1,
        pageSize: ITEMS_PAGE_SIZE,
      };
      const response = await searchTodoItems(searchRequest);
      if (response.isSuccess) {
        const searchResponse = response as unknown as SearchResponse<TodoItemData>;
        let data: TodoItemData[] = [];
        if (searchResponse.data?.data) {
          data = searchResponse.data.data.map((item: TodoItemData | { data: TodoItemData }) =>
            "data" in item ? item.data : item,
          );
        }
        setItems(data);
      } else {
        messageApi.error(response.message || "Không thể tải danh sách công việc");
      }
    } catch {
      messageApi.error("Có lỗi xảy ra khi tải danh sách công việc");
    } finally {
      setLoading(false);
    }
  }, [messageApi, todoListId]);

  useEffect(() => {
    fetchItems();
  }, [fetchItems]);

  const filteredItems = useMemo(() => {
    let result = items.filter((it) => {
      if (!searchText) return true;
      const field = searchField === "description" ? it.description : it.title;
      return (field || "").toLowerCase().includes(searchText.toLowerCase());
    });
    if (sortField) {
      result = [...result].sort((a, b) => {
        let av: string | number = "";
        let bv: string | number = "";
        if (sortField === "title") {
          av = a.title.toLowerCase();
          bv = b.title.toLowerCase();
        } else if (sortField === "priority") {
          av = a.priority;
          bv = b.priority;
        } else {
          av = a.dueDate || "";
          bv = b.dueDate || "";
        }
        if (av < bv) return sortAsc ? -1 : 1;
        if (av > bv) return sortAsc ? 1 : -1;
        return 0;
      });
    }
    return result;
  }, [items, searchText, searchField, sortField, sortAsc]);

  const handleSort = (field: SortField) => {
    if (sortField === field) setSortAsc((a) => !a);
    else {
      setSortField(field);
      setSortAsc(true);
    }
  };

  const startCreate = () => {
    setEditingItemId("new");
    setItemDraft(emptyDraft);
  };

  const startEdit = (item: TodoItemData) => {
    setEditingItemId(item.id);
    setItemDraft({
      title: item.title,
      description: item.description,
      priority: item.priority,
      dueDate: item.dueDate ? dayjs(item.dueDate).format("YYYY-MM-DD") : "",
    });
  };

  const cancelEdit = () => setEditingItemId(null);

  const buildRequest = (
    base: Partial<TodoItemData>,
    overrides: Partial<TodoItemData> = {},
  ): TodoItemRequest => {
    const merged = { ...base, ...overrides };
    return {
      id: merged.id,
      title: merged.title,
      description: merged.description,
      dueDate: merged.dueDate,
      isCompleted: merged.isCompleted ?? false,
      priority: merged.priority,
      completedOn: merged.completedOn,
      todoListId,
    } as unknown as TodoItemRequest;
  };

  const saveEdit = async () => {
    if (!itemDraft.title.trim()) {
      messageApi.warning("Vui lòng nhập tiêu đề");
      return;
    }
    if (!itemDraft.dueDate) {
      messageApi.warning("Vui lòng chọn hạn hoàn thành");
      return;
    }

    setSubmitting(true);
    try {
      if (editingItemId === "new") {
        const request = buildRequest({
          title: itemDraft.title.trim(),
          description: itemDraft.description.trim(),
          priority: itemDraft.priority,
          dueDate: itemDraft.dueDate,
          isCompleted: false,
        });
        const result = await createTodoItem(request);
        if (!result.isSuccess) {
          messageApi.error(result.message || "Tạo công việc thất bại");
          return;
        }
        messageApi.success(result.message || "Tạo công việc thành công");
      } else if (editingItemId) {
        const existing = items.find((it) => it.id === editingItemId);
        const request = buildRequest(existing || {}, {
          id: editingItemId,
          title: itemDraft.title.trim(),
          description: itemDraft.description.trim(),
          priority: itemDraft.priority,
          dueDate: itemDraft.dueDate,
        });
        const result = await updateTodoItem(request);
        if (!result.isSuccess) {
          messageApi.error(result.message || "Cập nhật công việc thất bại");
          return;
        }
        messageApi.success(result.message || "Cập nhật công việc thành công");
      }

      setEditingItemId(null);
      await fetchItems();
      onItemsChange();
    } finally {
      setSubmitting(false);
    }
  };

  const toggleComplete = async (item: TodoItemData) => {
    const nextCompleted = !item.isCompleted;
    const request = buildRequest(item, {
      isCompleted: nextCompleted,
      completedOn: nextCompleted ? dayjs().format("YYYY-MM-DD") : undefined,
    });
    const result = await updateTodoItem(request);
    if (!result.isSuccess) {
      messageApi.error(result.message || "Không thể cập nhật trạng thái");
      return;
    }
    await fetchItems();
    onItemsChange();
  };

  const choosePriority = async (item: TodoItemData, priority: Tier) => {
    setOpenPriorityId(null);
    if (priority === item.priority) return;
    const request = buildRequest(item, { priority });
    const result = await updateTodoItem(request);
    if (!result.isSuccess) {
      messageApi.error(result.message || "Không thể cập nhật độ ưu tiên");
      return;
    }
    await fetchItems();
  };

  const setDueDateInline = async (item: TodoItemData, value: string) => {
    setOpenDateId(null);
    if (!value) return;
    const request = buildRequest(item, { dueDate: value });
    const result = await updateTodoItem(request);
    if (!result.isSuccess) {
      messageApi.error(result.message || "Không thể cập nhật hạn hoàn thành");
      return;
    }
    await fetchItems();
  };

  const deleteItem = async (item: TodoItemData) => {
    const result = await deleteTodoItem(item.id);
    if (!result.isSuccess) {
      messageApi.error(result.message || "Xóa công việc thất bại");
      return;
    }
    await fetchItems();
    onItemsChange();

    const key = `undo-item-${item.id}-${Date.now()}`;
    notification.open({
      key,
      message: `Đã xoá "${item.title}"`,
      duration: 5,
      placement: "bottomRight",
      className: "cl-toast-notification",
      btn: (
        <Button
          size="small"
          type="link"
          icon={<UndoOutlined />}
          onClick={async () => {
            notification.destroy(key);
            const request = buildRequest(item);
            const recreate = await createTodoItem(request);
            if (recreate.isSuccess) {
              messageApi.success("Đã khôi phục công việc");
              await fetchItems();
              onItemsChange();
            } else {
              messageApi.error(recreate.message || "Không thể khôi phục công việc");
            }
          }}
        >
          Hoàn tác
        </Button>
      ),
    });
  };

  const totalAll = items.length;
  const completedAll = items.filter((it) => it.isCompleted).length;
  const highPriorityAll = items.filter((it) => it.priority === Tier.High && !it.isCompleted).length;

  const isCreating = editingItemId === "new";
  const newRowPlaceholder: TodoItemData & { _new: true } = {
    id: NEW_ROW_ID,
    _new: true,
    title: "",
    description: "",
    dueDate: "",
    isCompleted: false,
    priority: Tier.Medium,
  };
  const desktopDataSource: (TodoItemData & { _new?: boolean })[] = isCreating
    ? [newRowPlaceholder, ...filteredItems]
    : filteredItems;

  const priorityPopoverContent = (item: TodoItemData) => (
    <div className="cl-priority-popover">
      <button onClick={() => choosePriority(item, Tier.Low)}>Thấp</button>
      <button onClick={() => choosePriority(item, Tier.Medium)}>Trung bình</button>
      <button onClick={() => choosePriority(item, Tier.High)}>Cao</button>
    </div>
  );

  const columns = [
    {
      title: "",
      key: "checkbox",
      width: 34,
      render: (_: unknown, record: TodoItemData & { _new?: boolean }) =>
        record._new || record.id === editingItemId ? null : (
          <Checkbox checked={record.isCompleted} onChange={() => toggleComplete(record)} />
        ),
    },
    {
      title: <SortHeader label="Tiêu đề" field="title" sortField={sortField} sortAsc={sortAsc} onSort={handleSort} />,
      key: "title",
      render: (_: unknown, record: TodoItemData & { _new?: boolean }) => {
        if (record._new || record.id === editingItemId) {
          return (
            <Input
              autoFocus
              placeholder="Tiêu đề"
              value={itemDraft.title}
              onChange={(e) => setItemDraft({ ...itemDraft, title: e.target.value })}
            />
          );
        }
        return (
          <span
            className="cl-item-title"
            style={record.isCompleted ? { textDecoration: "line-through", opacity: 0.55 } : undefined}
          >
            {record.title}
          </span>
        );
      },
    },
    {
      title: "Mô tả",
      key: "description",
      render: (_: unknown, record: TodoItemData & { _new?: boolean }) =>
        record._new || record.id === editingItemId ? (
          <Input
            placeholder="Mô tả"
            value={itemDraft.description}
            onChange={(e) => setItemDraft({ ...itemDraft, description: e.target.value })}
          />
        ) : (
          <span className="text-muted">{record.description || "-"}</span>
        ),
    },
    {
      title: <SortHeader label="Độ ưu tiên" field="priority" sortField={sortField} sortAsc={sortAsc} onSort={handleSort} />,
      key: "priority",
      render: (_: unknown, record: TodoItemData & { _new?: boolean }) => {
        if (record._new || record.id === editingItemId) {
          return (
            <Select
              value={itemDraft.priority}
              style={{ width: 130 }}
              onChange={(value) => setItemDraft({ ...itemDraft, priority: value })}
              options={[
                { value: Tier.Low, label: "Thấp" },
                { value: Tier.Medium, label: "Trung bình" },
                { value: Tier.High, label: "Cao" },
              ]}
            />
          );
        }
        return (
          <Popover
            open={openPriorityId === record.id}
            onOpenChange={(open) => setOpenPriorityId(open ? record.id : null)}
            trigger="click"
            content={priorityPopoverContent(record)}
          >
            <span style={{ cursor: "pointer" }}>
              <PriorityTag priority={record.priority} />
            </span>
          </Popover>
        );
      },
    },
    {
      title: "Trạng thái",
      key: "status",
      render: (_: unknown, record: TodoItemData & { _new?: boolean }) =>
        record._new || record.id === editingItemId ? null : (
          <StatusTag isCompleted={record.isCompleted} isOverdue={isOverdue(record.dueDate, record.isCompleted)} />
        ),
    },
    {
      title: <SortHeader label="Hạn hoàn thành" field="dueDate" sortField={sortField} sortAsc={sortAsc} onSort={handleSort} />,
      key: "dueDate",
      render: (_: unknown, record: TodoItemData & { _new?: boolean }) => {
        if (record._new || record.id === editingItemId) {
          return (
            <input
              type="date"
              className="cl-date-input"
              value={itemDraft.dueDate}
              onChange={(e) => setItemDraft({ ...itemDraft, dueDate: e.target.value })}
            />
          );
        }
        if (openDateId === record.id) {
          return (
            <input
              type="date"
              className="cl-date-input"
              autoFocus
              defaultValue={record.dueDate ? dayjs(record.dueDate).format("YYYY-MM-DD") : ""}
              onChange={(e) => setDueDateInline(record, e.target.value)}
              onBlur={() => setOpenDateId(null)}
            />
          );
        }
        const overdue = isOverdue(record.dueDate, record.isCompleted);
        return (
          <button
            type="button"
            className="cl-date-trigger"
            style={overdue ? { color: "var(--cl-accent-700)", fontWeight: 600 } : undefined}
            onClick={() => setOpenDateId(record.id)}
          >
            <CalendarOutlined /> {formatDate(record.dueDate)}
          </button>
        );
      },
    },
    ...(showMoreColumns
      ? [
          {
            title: "Ngày hoàn thành",
            key: "completedOn",
            render: (_: unknown, record: TodoItemData & { _new?: boolean }) =>
              record._new || record.id === editingItemId ? null : (
                <span className="text-muted">{formatDate(record.completedOn)}</span>
              ),
          },
        ]
      : []),
    {
      title: "Thao tác",
      key: "action",
      width: 76,
      align: "center" as const,
      render: (_: unknown, record: TodoItemData & { _new?: boolean }) => {
        if (record._new || record.id === editingItemId) {
          return (
            <div className="cl-row-actions" style={{ opacity: 1, justifyContent: "center" }}>
              <Button size="small" onClick={cancelEdit}>
                Hủy
              </Button>
              <Button size="small" type="primary" loading={submitting} onClick={saveEdit}>
                Lưu
              </Button>
            </div>
          );
        }
        return (
          <div className="cl-row-actions" style={{ opacity: 1, justifyContent: "center" }}>
            <Button className="btn-icon-28" type="text" icon={<EditOutlined />} onClick={() => startEdit(record)} title="Sửa" />
            <Button className="btn-icon-28" type="text" icon={<DeleteOutlined />} onClick={() => deleteItem(record)} title="Xoá" />
          </div>
        );
      },
    },
  ];

  const showEmptyState = !loading && filteredItems.length === 0 && !isCreating;
  const emptyText = items.length > 0 ? "Không tìm thấy công việc phù hợp." : "Chưa có công việc nào trong danh sách này.";

  return (
    <div className="cl-panel cl-items-panel">
      <div className="cl-panel-header">
        <h2>{todoListName}</h2>
        <Button type="primary" icon={<PlusOutlined />} onClick={startCreate}>
          Thêm công việc
        </Button>
      </div>

      <div className="cl-stats-row">
        <div className="cl-stat-card">
          <span className="cl-stat-kicker">Tổng công việc</span>
          <span className="cl-stat-value">{totalAll}</span>
        </div>
        <div className="cl-stat-card">
          <span className="cl-stat-kicker">Đã hoàn thành</span>
          <span className="cl-stat-value">{completedAll}</span>
        </div>
        <div className="cl-stat-card">
          <span className="cl-stat-kicker">Ưu tiên cao</span>
          <span className="cl-stat-value">{highPriorityAll}</span>
        </div>
      </div>

      <div className="cl-items-toolbar">
        <Select
          value={searchField}
          style={{ width: 130 }}
          onChange={(value) => setSearchField(value)}
          options={[
            { value: "title", label: "Tiêu đề" },
            { value: "description", label: "Mô tả" },
          ]}
        />
        <div className="cl-search" style={{ flex: 1, minWidth: 180 }}>
          <SearchOutlined />
          <Input
            placeholder="Tìm công việc..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            allowClear
          />
        </div>
        <Button type="text" onClick={() => setShowMoreColumns((s) => !s)}>
          {showMoreColumns ? "Ẩn bớt" : "Hiện thêm"}
        </Button>
      </div>

      {loading ? (
        <div className="cl-rows">
          {[0, 1, 2].map((i) => (
            <div key={i} className="cl-skeleton-block" style={{ height: 52 }} />
          ))}
        </div>
      ) : showEmptyState ? (
        <div className="cl-empty">
          <CalendarOutlined style={{ fontSize: 30 }} />
          <p>{emptyText}</p>
        </div>
      ) : isDesktop ? (
        <div className="cl-table-wrap">
          <Table
            className="cl-table"
            columns={columns}
            dataSource={desktopDataSource}
            rowKey={(record) => record.id}
            pagination={false}
            scroll={{ y: 452, x: 760 }}
          />
        </div>
      ) : (
        <div className="cl-rows cl-item-cards">
          {isCreating && (
            <ItemInlineForm draft={itemDraft} onChange={setItemDraft} onCancel={cancelEdit} onSave={saveEdit} submitting={submitting} autoFocus />
          )}
          {filteredItems.map((item) =>
            editingItemId === item.id ? (
              <ItemInlineForm
                key={item.id}
                draft={itemDraft}
                onChange={setItemDraft}
                onCancel={cancelEdit}
                onSave={saveEdit}
                submitting={submitting}
                autoFocus
              />
            ) : (
              <div key={item.id} className="cl-item-card">
                <div className="cl-item-card-top">
                  <Checkbox checked={item.isCompleted} onChange={() => toggleComplete(item)} />
                  <div className="cl-item-card-text">
                    <span
                      className="cl-item-title"
                      style={item.isCompleted ? { textDecoration: "line-through", opacity: 0.55 } : undefined}
                    >
                      {item.title}
                    </span>
                    {item.description && <span className="text-muted cl-item-card-desc">{item.description}</span>}
                  </div>
                  <div className="cl-row-actions" style={{ opacity: 1 }}>
                    <Button className="btn-icon-28" type="text" icon={<EditOutlined />} onClick={() => startEdit(item)} />
                    <Button className="btn-icon-28" type="text" icon={<DeleteOutlined />} onClick={() => deleteItem(item)} />
                  </div>
                </div>
                <div className="cl-item-card-meta">
                  <PriorityTag priority={item.priority} />
                  <StatusTag isCompleted={item.isCompleted} isOverdue={isOverdue(item.dueDate, item.isCompleted)} />
                  <span
                    className="text-muted cl-item-card-date"
                    style={isOverdue(item.dueDate, item.isCompleted) ? { color: "var(--cl-accent-700)", fontWeight: 600 } : undefined}
                  >
                    {formatDate(item.dueDate)}
                  </span>
                </div>
              </div>
            ),
          )}
        </div>
      )}
    </div>
  );
};

export default TodoItems;
