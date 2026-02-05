import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Building2, Clock } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Select, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { ActivitiesTimeline } from '@/components/crm/ActivitiesTimeline';

interface Partner {
    id: string;
    code: string;
    name: string;
    isCustomer: boolean;
    isSupplier: boolean;
    email: string | null;
    mobilePhone: string | null;
    isActive: boolean;
    createdAt: string;
}

interface PartnerFormData {
    code: string;
    name: string;
    isCustomer: boolean;
    isSupplier: boolean;
    kind: string;
    email: string;
    mobilePhone: string;
    taxOffice?: string;
    taxNumber?: string;
    identityNumber?: string;
}

export function PartnersPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [partners, setPartners] = useState<Partner[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingPartner, setEditingPartner] = useState<Partner | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Activities Modal state
    const [isActivitiesModalOpen, setIsActivitiesModalOpen] = useState(false);
    const [selectedPartnerForActivities, setSelectedPartnerForActivities] = useState<Partner | null>(null);

    // Form state
    const [formData, setFormData] = useState<PartnerFormData>({
        code: '',
        name: '',
        isCustomer: true,
        isSupplier: false,
        kind: 'Company',
        email: '',
        mobilePhone: '',
    });
    const [formErrors, setFormErrors] = useState<Partial<PartnerFormData>>({});

    const loadPartners = useCallback(async () => {
        setIsLoading(true);
        const result = await api.partners.getAll(page, pageSize);
        if (result.success && result.data) {
            setPartners(result.data.data);
            setTotal(result.data.totalCount);
        }
        setIsLoading(false);
    }, [page]);

    useEffect(() => {
        loadPartners();
    }, [loadPartners]);

    const openCreateModal = () => {
        setEditingPartner(null);
        setFormData({
            code: '',
            name: '',
            isCustomer: true,
            isSupplier: false,
            kind: 'Company',
            email: '',
            mobilePhone: '',
            taxOffice: '',
            taxNumber: '',
            identityNumber: '',
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openEditModal = (partner: Partner) => {
        setEditingPartner(partner);
        setFormData({
            code: partner.code,
            name: partner.name,
            isCustomer: partner.isCustomer,
            isSupplier: partner.isSupplier,
            kind: 'Company',
            email: partner.email || '',
            mobilePhone: partner.mobilePhone || '',
            // Populate if available (Note: List DTO might lack these, will need Detail fetch in future)
            taxOffice: (partner as any).taxOffice || '',
            taxNumber: (partner as any).taxNumber || '',
            identityNumber: (partner as any).identityNumber || '',
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openActivitiesModal = (partner: Partner) => {
        setSelectedPartnerForActivities(partner);
        setIsActivitiesModalOpen(true);
    };

    const validateForm = (): boolean => {
        const errors: Partial<PartnerFormData> = {};

        if (!formData.code.trim()) {
            errors.code = t('common.required');
        }
        if (!formData.name.trim()) {
            errors.name = t('common.required');
        }
        if (!formData.isCustomer && !formData.isSupplier) {
            errors.name = t('partners.mustBeCustomerOrSupplier');
        }

        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async () => {
        if (!validateForm()) return;

        setIsSubmitting(true);
        try {
            if (editingPartner) {
                const result = await api.partners.update(editingPartner.id, formData);
                if (result.success) {
                    toast.success(t('partners.partnerUpdated'));
                    setIsModalOpen(false);
                    loadPartners();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } else {
                const result = await api.partners.create(formData);
                if (result.success) {
                    toast.success(t('partners.partnerCreated'));
                    setIsModalOpen(false);
                    loadPartners();
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

    const handleDelete = async (partner: Partner) => {
        const confirmed = await dialog.danger({
            title: t('partners.deletePartner'),
            message: t('partners.confirmDeletePartner'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.partners.delete(partner.id);
            if (result.success) {
                toast.success(t('partners.partnerDeleted'));
                loadPartners();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const columns: Column<Partner>[] = [
        {
            key: 'partner',
            header: t('partners.name'),
            render: (partner) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600 flex items-center justify-center">
                        <Building2 className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-medium">{partner.name}</p>
                        <p className="text-sm text-[hsl(var(--muted-foreground))]">{partner.code}</p>
                    </div>
                </div>
            ),
        },
        {
            key: 'type',
            header: t('partners.type'),
            render: (partner) => (
                <div className="flex gap-1">
                    {partner.isCustomer && (
                        <Badge variant="success">{t('partners.customer')}</Badge>
                    )}
                    {partner.isSupplier && (
                        <Badge variant="info">{t('partners.supplier')}</Badge>
                    )}
                </div>
            ),
        },
        {
            key: 'contact',
            header: t('partners.email'),
            render: (partner) => (
                <div className="text-sm">
                    {partner.email && <p>{partner.email}</p>}
                    {partner.mobilePhone && <p className="text-[hsl(var(--muted-foreground))]">{partner.mobilePhone}</p>}
                    {!partner.email && !partner.mobilePhone && '-'}
                </div>
            ),
        },
        {
            key: 'isActive',
            header: t('common.status'),
            render: (partner) => (
                <Badge variant={partner.isActive ? 'success' : 'error'}>
                    {partner.isActive ? t('common.active') : t('common.inactive')}
                </Badge>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('partners.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('partners.subtitle')}
                    </p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('partners.createPartner')}
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={partners}
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
                actions={(partner) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => openActivitiesModal(partner)}
                            className="p-2 rounded-lg hover:bg-blue-100 dark:hover:bg-blue-900/30 text-blue-600 transition-colors"
                            title={t('activities.title')}
                        >
                            <Clock className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => openEditModal(partner)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(partner)}
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
                title={selectedPartnerForActivities ? `${t('activities.title')} - ${selectedPartnerForActivities.name}` : t('activities.title')}
                size="lg"
            >
                {selectedPartnerForActivities && (
                    <ActivitiesTimeline
                        entityId={selectedPartnerForActivities.id}
                        entityType="BusinessPartner"
                    />
                )}
            </Modal>

            {/* Create/Edit Modal */}
            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingPartner ? t('partners.editPartner') : t('partners.createPartner')}
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
                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('partners.code')}
                            value={formData.code}
                            onChange={(e) => setFormData({ ...formData, code: e.target.value })}
                            error={formErrors.code}
                            disabled={!!editingPartner}
                            required
                        />
                        <Select
                            label={t('partners.kind')}
                            value={formData.kind}
                            onChange={(e) => setFormData({ ...formData, kind: e.target.value })}
                            options={[
                                { value: 'Company', label: t('partners.company') },
                                { value: 'Individual', label: t('partners.individual') }
                            ]}
                        />
                    </div>
                    <Input
                        label={t('partners.name')}
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                        error={formErrors.name}
                        required
                    />

                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('partners.taxOffice')}
                            value={formData.taxOffice}
                            onChange={(e) => setFormData({ ...formData, taxOffice: e.target.value })}
                        />
                        {formData.kind === 'Company' ? (
                            <Input
                                label={t('partners.taxNumber')}
                                value={formData.taxNumber}
                                onChange={(e) => setFormData({ ...formData, taxNumber: e.target.value })}
                            />
                        ) : (
                            <Input
                                label={t('partners.identityNumber')}
                                value={formData.identityNumber}
                                onChange={(e) => setFormData({ ...formData, identityNumber: e.target.value })}
                            />
                        )}
                    </div>

                    <div className="flex gap-6">
                        <label className="flex items-center gap-2 cursor-pointer">
                            <input
                                type="checkbox"
                                checked={formData.isCustomer}
                                onChange={(e) => setFormData({ ...formData, isCustomer: e.target.checked })}
                                className="rounded border-[hsl(var(--border))]"
                            />
                            <span>{t('partners.isCustomer')}</span>
                        </label>
                        <label className="flex items-center gap-2 cursor-pointer">
                            <input
                                type="checkbox"
                                checked={formData.isSupplier}
                                onChange={(e) => setFormData({ ...formData, isSupplier: e.target.checked })}
                                className="rounded border-[hsl(var(--border))]"
                            />
                            <span>{t('partners.isSupplier')}</span>
                        </label>
                    </div>
                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('partners.email')}
                            type="email"
                            value={formData.email}
                            onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                        />
                        <Input
                            label={t('partners.mobilePhone')}
                            value={formData.mobilePhone}
                            onChange={(e) => setFormData({ ...formData, mobilePhone: e.target.value })}
                        />
                    </div>
                </div>
            </Modal>
        </div>
    );
}
