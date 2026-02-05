import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, MessageCircle, CheckCircle, XCircle, RotateCcw, AlertCircle, Send } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Select, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface Ticket {
    id: string;
    title: string;
    priority: number;
    priorityName: string;
    status: number;
    statusName: string;
    partnerId: string | null;
    partnerName: string | null;
    assignedUserId: string | null;
    createdAt: string;
}

interface TicketDetail extends Ticket {
    description: string;
    resolution: string | null;
    resolvedAt: string | null;
    closedAt: string | null;
}

interface TicketMessage {
    id: string;
    senderUserId: string | null;
    senderName: string | null;
    message: string;
    isInternal: boolean;
    createdAt: string;
}

interface TicketFormData {
    title: string;
    description: string;
    priority: number;
    partnerId?: string;
}

const PRIORITIES = [
    { value: 0, label: 'Low', color: 'text-green-600' },
    { value: 1, label: 'Medium', color: 'text-yellow-600' },
    { value: 2, label: 'High', color: 'text-orange-600' },
    { value: 3, label: 'Critical', color: 'text-red-600' },
];

const STATUSES = [
    { value: 0, label: 'Open', variant: 'info' as const },
    { value: 1, label: 'In Progress', variant: 'warning' as const },
    { value: 2, label: 'Resolved', variant: 'success' as const },
    { value: 3, label: 'Closed', variant: 'default' as const },
];

export function TicketsPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [tickets, setTickets] = useState<Ticket[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    // Create/Edit Modal
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingTicket, setEditingTicket] = useState<Ticket | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [formData, setFormData] = useState<TicketFormData>({
        title: '',
        description: '',
        priority: 1,
    });

    // Detail/Messages Modal
    const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
    const [selectedTicket, setSelectedTicket] = useState<TicketDetail | null>(null);
    const [messages, setMessages] = useState<TicketMessage[]>([]);
    const [newMessage, setNewMessage] = useState('');
    const [isInternal, setIsInternal] = useState(false);
    const [isLoadingMessages, setIsLoadingMessages] = useState(false);
    const [isSendingMessage, setIsSendingMessage] = useState(false);

    // Resolve Modal
    const [isResolveModalOpen, setIsResolveModalOpen] = useState(false);
    const [resolutionText, setResolutionText] = useState('');

    const loadTickets = useCallback(async () => {
        setIsLoading(true);
        const result = await api.tickets.getAll(page, pageSize);
        if (result.success && result.data) {
            setTickets(result.data.data);
            setTotal(result.data.totalCount);
        }
        setIsLoading(false);
    }, [page]);

    useEffect(() => {
        loadTickets();
    }, [loadTickets]);

    const openCreateModal = () => {
        setEditingTicket(null);
        setFormData({ title: '', description: '', priority: 1 });
        setIsModalOpen(true);
    };

    const openEditModal = (ticket: Ticket) => {
        setEditingTicket(ticket);
        setFormData({
            title: ticket.title,
            description: '',
            priority: ticket.priority,
        });
        setIsModalOpen(true);
    };

    const openDetailModal = async (ticket: Ticket) => {
        setIsDetailModalOpen(true);
        setIsLoadingMessages(true);

        const detailResult = await api.tickets.getById(ticket.id);
        if (detailResult.success && detailResult.data) {
            setSelectedTicket(detailResult.data);
        }

        const messagesResult = await api.tickets.getMessages(ticket.id);
        if (messagesResult.success && messagesResult.data) {
            setMessages(messagesResult.data);
        }
        setIsLoadingMessages(false);
    };

    const handleSubmit = async () => {
        if (!formData.title.trim()) {
            toast.error(t('common.error'), t('common.required'));
            return;
        }

        setIsSubmitting(true);
        try {
            if (editingTicket) {
                const result = await api.tickets.update(editingTicket.id, {
                    ...formData,
                    title: formData.title,
                    description: formData.description,
                    priority: formData.priority,
                });
                if (result.success) {
                    toast.success(t('tickets.ticketUpdated'));
                    setIsModalOpen(false);
                    loadTickets();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } else {
                const result = await api.tickets.create(formData);
                if (result.success) {
                    toast.success(t('tickets.ticketCreated'));
                    setIsModalOpen(false);
                    loadTickets();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            }
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = async (ticket: Ticket) => {
        const confirmed = await dialog.danger({
            title: t('tickets.deleteTicket'),
            message: t('tickets.confirmDeleteTicket'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.tickets.delete(ticket.id);
            if (result.success) {
                toast.success(t('tickets.ticketDeleted'));
                loadTickets();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const handleResolve = async () => {
        if (!selectedTicket || !resolutionText.trim()) return;

        const result = await api.tickets.resolve(selectedTicket.id, resolutionText);
        if (result.success) {
            toast.success(t('tickets.ticketResolved'));
            setIsResolveModalOpen(false);
            setResolutionText('');
            setSelectedTicket(result.data);
            loadTickets();
        } else {
            toast.error(t('common.error'), result.error);
        }
    };

    const handleClose = async () => {
        if (!selectedTicket) return;

        const result = await api.tickets.close(selectedTicket.id);
        if (result.success) {
            toast.success(t('tickets.ticketClosed'));
            setSelectedTicket(result.data);
            loadTickets();
        } else {
            toast.error(t('common.error'), result.error);
        }
    };

    const handleReopen = async () => {
        if (!selectedTicket) return;

        const result = await api.tickets.reopen(selectedTicket.id);
        if (result.success) {
            toast.success(t('tickets.ticketReopened'));
            setSelectedTicket(result.data);
            loadTickets();
        } else {
            toast.error(t('common.error'), result.error);
        }
    };

    const handleSendMessage = async () => {
        if (!selectedTicket || !newMessage.trim()) return;

        setIsSendingMessage(true);
        const result = await api.tickets.addMessage(selectedTicket.id, {
            message: newMessage,
            isInternal,
        });
        if (result.success) {
            setNewMessage('');
            setIsInternal(false);
            // Reload messages
            const messagesResult = await api.tickets.getMessages(selectedTicket.id);
            if (messagesResult.success && messagesResult.data) {
                setMessages(messagesResult.data);
            }
        } else {
            toast.error(t('common.error'), result.error);
        }
        setIsSendingMessage(false);
    };

    const getPriorityBadge = (priority: number) => {
        const p = PRIORITIES.find(pr => pr.value === priority) || PRIORITIES[1];
        const variants: Record<number, "info" | "warning" | "success" | "error"> = {
            0: 'success', 1: 'warning', 2: 'warning', 3: 'error'
        };
        return <Badge variant={variants[priority] || 'info'}>{p.label}</Badge>;
    };

    const getStatusBadge = (status: number) => {
        const s = STATUSES.find(st => st.value === status) || STATUSES[0];
        return <Badge variant={s.variant}>{s.label}</Badge>;
    };

    const columns: Column<Ticket>[] = [
        {
            key: 'title',
            header: t('tickets.title'),
            render: (ticket) => (
                <div>
                    <p className="font-medium">{ticket.title}</p>
                    <p className="text-sm text-[hsl(var(--muted-foreground))]">
                        {new Date(ticket.createdAt).toLocaleDateString()}
                    </p>
                </div>
            ),
        },
        {
            key: 'priority',
            header: t('tickets.priority'),
            render: (ticket) => getPriorityBadge(ticket.priority),
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (ticket) => getStatusBadge(ticket.status),
        },
        {
            key: 'partner',
            header: t('tickets.partner'),
            render: (ticket) => ticket.partnerName || '-',
        },
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('tickets.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('tickets.subtitle')}</p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('tickets.createTicket')}
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={tickets}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                pagination={{ page, pageSize, total, onPageChange: setPage }}
                actions={(ticket) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => openDetailModal(ticket)}
                            className="p-2 rounded-lg hover:bg-blue-100 dark:hover:bg-blue-900/30 text-blue-600 transition-colors"
                            title={t('tickets.viewConversation')}
                        >
                            <MessageCircle className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => openEditModal(ticket)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(ticket)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            {/* Create/Edit Modal */}
            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingTicket ? t('tickets.editTicket') : t('tickets.createTicket')}
                size="md"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit} isLoading={isSubmitting}>{t('common.save')}</Button>
                    </>
                }
            >
                <div className="space-y-4">
                    <Input
                        label={t('tickets.ticketTitle')}
                        value={formData.title}
                        onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                        required
                    />
                    <div>
                        <label className="block text-sm font-medium mb-2">{t('tickets.description')}</label>
                        <textarea
                            value={formData.description}
                            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                            rows={4}
                            className="w-full px-3 py-2 border border-[hsl(var(--border))] rounded-lg bg-[hsl(var(--background))]"
                        />
                    </div>
                    <Select
                        label={t('tickets.priority')}
                        value={String(formData.priority)}
                        onChange={(e) => setFormData({ ...formData, priority: Number(e.target.value) })}
                        options={PRIORITIES.map(p => ({ value: String(p.value), label: p.label }))}
                    />
                </div>
            </Modal>

            {/* Detail/Conversation Modal */}
            <Modal
                isOpen={isDetailModalOpen}
                onClose={() => { setIsDetailModalOpen(false); setSelectedTicket(null); setMessages([]); }}
                title={selectedTicket?.title || t('tickets.ticketDetails')}
                size="lg"
            >
                {selectedTicket && (
                    <div className="space-y-4">
                        {/* Ticket Info */}
                        <div className="flex items-center gap-2 flex-wrap">
                            {getPriorityBadge(selectedTicket.priority)}
                            {getStatusBadge(selectedTicket.status)}
                            {selectedTicket.partnerName && (
                                <span className="text-sm text-[hsl(var(--muted-foreground))]">
                                    • {selectedTicket.partnerName}
                                </span>
                            )}
                        </div>

                        {selectedTicket.description && (
                            <p className="text-sm bg-[hsl(var(--accent))] p-3 rounded-lg">{selectedTicket.description}</p>
                        )}

                        {selectedTicket.resolution && (
                            <div className="bg-green-50 dark:bg-green-900/20 p-3 rounded-lg border border-green-200 dark:border-green-800">
                                <p className="text-sm font-medium text-green-700 dark:text-green-300">{t('tickets.resolution')}:</p>
                                <p className="text-sm">{selectedTicket.resolution}</p>
                            </div>
                        )}

                        {/* Actions */}
                        <div className="flex gap-2 flex-wrap">
                            {selectedTicket.status < 2 && (
                                <Button size="sm" onClick={() => setIsResolveModalOpen(true)}>
                                    <CheckCircle className="w-4 h-4" /> {t('tickets.resolve')}
                                </Button>
                            )}
                            {selectedTicket.status === 2 && (
                                <Button size="sm" onClick={handleClose}>
                                    <XCircle className="w-4 h-4" /> {t('tickets.close')}
                                </Button>
                            )}
                            {selectedTicket.status >= 2 && (
                                <Button size="sm" variant="secondary" onClick={handleReopen}>
                                    <RotateCcw className="w-4 h-4" /> {t('tickets.reopen')}
                                </Button>
                            )}
                        </div>

                        {/* Messages */}
                        <div className="border-t pt-4">
                            <h4 className="font-medium mb-3">{t('tickets.conversation')}</h4>
                            {isLoadingMessages ? (
                                <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('common.loading')}</p>
                            ) : (
                                <div className="space-y-3 max-h-60 overflow-y-auto">
                                    {messages.length === 0 ? (
                                        <p className="text-sm text-[hsl(var(--muted-foreground))]">{t('tickets.noMessages')}</p>
                                    ) : (
                                        messages.map((msg) => (
                                            <div
                                                key={msg.id}
                                                className={`p-3 rounded-lg ${msg.isInternal
                                                    ? 'bg-yellow-50 dark:bg-yellow-900/20 border border-yellow-200 dark:border-yellow-800'
                                                    : 'bg-[hsl(var(--accent))]'
                                                    }`}
                                            >
                                                <div className="flex items-center gap-2 mb-1">
                                                    {msg.isInternal && <AlertCircle className="w-3 h-3 text-yellow-600" />}
                                                    <span className="text-xs font-medium">{msg.senderName || 'System'}</span>
                                                    <span className="text-xs text-[hsl(var(--muted-foreground))]">
                                                        {new Date(msg.createdAt).toLocaleString()}
                                                    </span>
                                                </div>
                                                <p className="text-sm">{msg.message}</p>
                                            </div>
                                        ))
                                    )}
                                </div>
                            )}

                            {/* Add Message */}
                            {selectedTicket.status < 3 && (
                                <div className="mt-4 space-y-2">
                                    <textarea
                                        value={newMessage}
                                        onChange={(e) => setNewMessage(e.target.value)}
                                        placeholder={t('tickets.typeMessage')}
                                        rows={2}
                                        className="w-full px-3 py-2 border border-[hsl(var(--border))] rounded-lg bg-[hsl(var(--background))]"
                                    />
                                    <div className="flex items-center justify-between">
                                        <label className="flex items-center gap-2 text-sm cursor-pointer">
                                            <input
                                                type="checkbox"
                                                checked={isInternal}
                                                onChange={(e) => setIsInternal(e.target.checked)}
                                                className="rounded"
                                            />
                                            {t('tickets.internalNote')}
                                        </label>
                                        <Button size="sm" onClick={handleSendMessage} isLoading={isSendingMessage}>
                                            <Send className="w-4 h-4" /> {t('tickets.send')}
                                        </Button>
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>
                )}
            </Modal>

            {/* Resolve Modal */}
            <Modal
                isOpen={isResolveModalOpen}
                onClose={() => setIsResolveModalOpen(false)}
                title={t('tickets.resolveTicket')}
                size="md"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsResolveModalOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleResolve}>{t('tickets.resolve')}</Button>
                    </>
                }
            >
                <div>
                    <label className="block text-sm font-medium mb-2">{t('tickets.resolution')}</label>
                    <textarea
                        value={resolutionText}
                        onChange={(e) => setResolutionText(e.target.value)}
                        rows={4}
                        placeholder={t('tickets.enterResolution')}
                        className="w-full px-3 py-2 border border-[hsl(var(--border))] rounded-lg bg-[hsl(var(--background))]"
                    />
                </div>
            </Modal>
        </div>
    );
}
