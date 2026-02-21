import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Building2, Clock } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Badge, useToast, useDialog } from '@/components/ui';
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


export function PartnersPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();
    const navigate = useNavigate();

    const [partners, setPartners] = useState<Partner[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    // Activities Modal state
    const [isActivitiesModalOpen, setIsActivitiesModalOpen] = useState(false);
    const [selectedPartnerForActivities, setSelectedPartnerForActivities] = useState<Partner | null>(null);

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
        navigate('/partners/new');
    };

    const openEditModal = (partner: Partner) => {
        navigate(`/partners/${partner.id}`);
    };

    const openActivitiesModal = (partner: Partner) => {
        setSelectedPartnerForActivities(partner);
        setIsActivitiesModalOpen(true);
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

            {/* Modals have been removed and replaced with BusinessPartnerDetail.tsx navigation routes */}
        </div>
    );
}
