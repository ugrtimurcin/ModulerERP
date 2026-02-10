import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Target, Phone, Mail, Building2, ArrowRight, Clock } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Badge, useToast, useDialog, Select } from '@/components/ui';
import type { Column } from '@/components/ui';
import { ActivitiesTimeline } from '@/components/crm/ActivitiesTimeline';

interface Lead {
    id: string;
    title: string | null;
    firstName: string;
    lastName: string;
    fullName: string;
    company: string | null;
    email: string | null;
    phone: string | null;
    status: 'New' | 'Contacted' | 'Qualified' | 'Junk' | 'Converted';
    source: string | null;
    assignedUserId: string | null;
    isActive: boolean;
    createdAt: string;
}

interface LeadFormData {
    title: string;
    firstName: string;
    lastName: string;
    company: string;
    email: string;
    phone: string;
    status: string;
    source: string;
}

export function LeadsPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [leads, setLeads] = useState<Lead[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingLead, setEditingLead] = useState<Lead | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Activities Modal state
    const [isActivitiesModalOpen, setIsActivitiesModalOpen] = useState(false);
    const [selectedLeadForActivities, setSelectedLeadForActivities] = useState<Lead | null>(null);

    // Form state
    const [formData, setFormData] = useState<LeadFormData>({
        title: '',
        firstName: '',
        lastName: '',
        company: '',
        email: '',
        phone: '',
        status: 'New',
        source: '',
    });
    const [formErrors, setFormErrors] = useState<Partial<LeadFormData>>({});

    const loadLeads = useCallback(async () => {
        setIsLoading(true);
        const result = await api.leads.getAll(page, pageSize);
        if (result.success && result.data) {
            setLeads(result.data.data as unknown as Lead[]);
            setTotal(result.data.totalCount);
        }
        setIsLoading(false);
    }, [page]);

    useEffect(() => {
        loadLeads();
    }, [loadLeads]);

    const openCreateModal = () => {
        setEditingLead(null);
        setFormData({
            title: '',
            firstName: '',
            lastName: '',
            company: '',
            email: '',
            phone: '',
            status: 'New',
            source: '',
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openEditModal = (lead: Lead) => {
        setEditingLead(lead);
        setFormData({
            title: lead.title || '',
            firstName: lead.firstName,
            lastName: lead.lastName,
            company: lead.company || '',
            email: lead.email || '',
            phone: lead.phone || '',
            status: lead.status,
            source: lead.source || '',
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openActivitiesModal = (lead: Lead) => {
        setSelectedLeadForActivities(lead);
        setIsActivitiesModalOpen(true);
    };

    const validateForm = (): boolean => {
        const errors: Partial<LeadFormData> = {};

        if (!formData.firstName.trim()) {
            errors.firstName = t('common.required');
        }
        if (!formData.lastName.trim()) {
            errors.lastName = t('common.required');
        }

        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async () => {
        if (!validateForm()) return;

        setIsSubmitting(true);
        try {
            if (editingLead) {
                const result = await api.leads.update(editingLead.id, formData);
                if (result.success) {
                    toast.success(t('leads.leadUpdated'));
                    setIsModalOpen(false);
                    loadLeads();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } else {
                const result = await api.leads.create(formData);
                if (result.success) {
                    toast.success(t('leads.leadCreated'));
                    setIsModalOpen(false);
                    loadLeads();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            }
        } catch (err) {
            toast.error(t('common.error'), (err as Error).message);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = async (lead: Lead) => {
        const confirmed = await dialog.danger({
            title: t('leads.deleteLead'),
            message: t('leads.confirmDeleteLead'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.leads.delete(lead.id);
            if (result.success) {
                toast.success(t('leads.leadDeleted'));
                loadLeads();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const handleConvert = async (lead: Lead) => {
        const confirmed = await dialog.confirm({
            title: t('leads.convert'),
            message: t('leads.convertConfirmation', 'Are you sure you want to convert this lead to a business partner?'),
            confirmText: t('common.confirm'),
        });

        if (confirmed) {
            try {
                const result = await api.leads.convert(lead.id);
                if (result.success) {
                    toast.success(t('leads.convertedSuccess'));
                    loadLeads();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } catch (err) {
                toast.error(t('common.error'), (err as Error).message);
            }
        }
    };

    const getStatusBadge = (status: string) => {
        const variants: Record<string, "info" | "success" | "warning" | "error" | "default"> = {
            New: "info",
            Contacted: "warning",
            Qualified: "success",
            Junk: "error",
            Converted: "default"
        };
        return <Badge variant={variants[status] || "default"}>{t(`leads.${status}`)}</Badge>;
    };

    const columns: Column<Lead>[] = [
        {
            key: 'name',
            header: t('leads.firstName'),
            render: (lead) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-full bg-blue-100 dark:bg-blue-900/30 text-blue-600 flex items-center justify-center">
                        <Target className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-medium">{lead.fullName}</p>
                        {lead.company && (
                            <div className="flex items-center gap-1 text-sm text-[hsl(var(--muted-foreground))]">
                                <Building2 className="w-3 h-3" />
                                <span>{lead.company}</span>
                            </div>
                        )}
                    </div>
                </div>
            ),
        },
        {
            key: 'contact',
            header: t('leads.email'),
            render: (lead) => (
                <div className="space-y-1 text-sm">
                    {lead.email && (
                        <div className="flex items-center gap-2 text-[hsl(var(--muted-foreground))]">
                            <Mail className="w-3 h-3" />
                            <span>{lead.email}</span>
                        </div>
                    )}
                    {lead.phone && (
                        <div className="flex items-center gap-2 text-[hsl(var(--muted-foreground))]">
                            <Phone className="w-3 h-3" />
                            <span>{lead.phone}</span>
                        </div>
                    )}
                </div>
            ),
        },
        {
            key: 'status',
            header: t('leads.status'),
            render: (lead) => getStatusBadge(lead.status),
        },
        {
            key: 'source',
            header: t('leads.source'),
            render: (lead) => <span className="text-sm">{lead.source || '-'}</span>,
        }
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('leads.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('leads.subtitle')}
                    </p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('leads.createLead')}
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={leads}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                pagination={{
                    page,
                    pageSize,
                    total,
                    onPageChange: setPage,
                }}
                actions={(lead) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => openActivitiesModal(lead)}
                            className="p-2 rounded-lg hover:bg-blue-100 dark:hover:bg-blue-900/30 text-blue-600 transition-colors"
                            title={t('activities.title')}
                        >
                            <Clock className="w-4 h-4" />
                        </button>
                        {lead.status !== 'Converted' && lead.status !== 'Junk' && (
                            <button
                                onClick={() => handleConvert(lead)}
                                className="p-2 rounded-lg hover:bg-green-100 dark:hover:bg-green-900/30 text-green-600 transition-colors"
                                title={t('leads.convert')}
                            >
                                <ArrowRight className="w-4 h-4" />
                            </button>
                        )}
                        <button
                            onClick={() => openEditModal(lead)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                            disabled={lead.status === 'Converted'}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>

                        <button
                            onClick={() => handleDelete(lead)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            {/* Activities Modal */}
            <Modal
                isOpen={isActivitiesModalOpen}
                onClose={() => setIsActivitiesModalOpen(false)}
                title={selectedLeadForActivities ? `${t('activities.title')} - ${selectedLeadForActivities.fullName}` : t('activities.title')}
                size="lg"
            >
                {selectedLeadForActivities && (
                    <ActivitiesTimeline
                        entityId={selectedLeadForActivities.id}
                        entityType="Lead"
                    />
                )}
            </Modal>

            {/* Create/Edit Modal */}
            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingLead ? t('leads.editLead') : t('leads.createLead')}
                size="md"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button onClick={handleSubmit} isLoading={isSubmitting}>
                            {t('common.save')}
                        </Button>
                    </>
                }
            >
                <div className="space-y-4">
                    <div className="flex gap-4">
                        <div className="w-1/4">
                            <Input
                                label={t('common.title')}
                                value={formData.title}
                                onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                                placeholder={t('leads.titlePlaceholder')}
                            />
                        </div>
                        <div className="flex-1">
                            <Input
                                label={t('leads.firstName')}
                                value={formData.firstName}
                                onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                                error={formErrors.firstName}
                                required
                            />
                        </div>
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('leads.lastName')}
                            value={formData.lastName}
                            onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                            error={formErrors.lastName}
                            required
                        />
                    </div>
                    <Input
                        label={t('leads.company')}
                        value={formData.company}
                        onChange={(e) => setFormData({ ...formData, company: e.target.value })}
                    />
                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('leads.email')}
                            type="email"
                            value={formData.email}
                            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                        />
                        <Input
                            label={t('leads.phone')}
                            value={formData.phone}
                            onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                        />
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                        <Select
                            label={t('leads.status')}
                            value={formData.status}
                            onChange={(e) => setFormData({ ...formData, status: e.target.value })}
                            options={[
                                { value: 'New', label: t('leads.New') },
                                { value: 'Contacted', label: t('leads.Contacted') },
                                { value: 'Qualified', label: t('leads.Qualified') },
                                { value: 'Junk', label: t('leads.Junk') },
                            ]}
                            disabled={formData.status === 'Converted'}
                        />
                        <Input
                            label={t('leads.source')}
                            value={formData.source}
                            onChange={(e) => setFormData({ ...formData, source: e.target.value })}
                            placeholder="e.g. Website, LinkedIn"
                        />
                    </div>
                </div>
            </Modal>
        </div>
    );
}
