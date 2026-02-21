import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, TrendingUp, Building2 } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { api } from '@/services/api';
import { DataTable, Button, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface Opportunity {
    id: string;
    title: string;
    partnerId: string | null;
    partnerName: string | null;
    estimatedValue: number;
    currencyCode: string;
    stage: 'Discovery' | 'Proposal' | 'Negotiation' | 'Won' | 'Lost';
    probability: number;
    weightedValue: number;
    expectedCloseDate: string | null;
    assignedUserId: string | null;
    isActive: boolean;
    createdAt: string;
}

export function OpportunitiesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();
    const navigate = useNavigate();

    const [opportunities, setOpportunities] = useState<Opportunity[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    const loadOpportunities = useCallback(async () => {
        setIsLoading(true);
        const result = await api.opportunities.getAll(page, pageSize);
        if (result.success && result.data) {
            setOpportunities(result.data.data as Opportunity[]);
            setTotal(result.data.totalCount);
        }
        setIsLoading(false);
    }, [page]);

    useEffect(() => {
        loadOpportunities();
    }, [loadOpportunities]);

    const openCreateModal = () => {
        navigate('/opportunities/new');
    };

    const openEditModal = (opp: Opportunity) => {
        navigate(`/opportunities/${opp.id}`);
    };

    const handleDelete = async (opp: Opportunity) => {
        const confirmed = await dialog.danger({
            title: t('opportunities.deleteOpportunity'),
            message: t('opportunities.confirmDeleteOpportunity'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.opportunities.delete(opp.id);
            if (result.success) {
                toast.success(t('opportunities.opportunityDeleted'));
                loadOpportunities();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const getStageBadge = (stage: string) => {
        const variants: Record<string, "info" | "success" | "warning" | "error" | "default"> = {
            Discovery: "info",
            Proposal: "warning",
            Negotiation: "default",
            Won: "success",
            Lost: "error"
        };
        return <Badge variant={variants[stage] || "default"}>{t(`opportunities.${stage}`)}</Badge>;
    };

    const columns: Column<Opportunity>[] = [
        {
            key: 'title',
            header: t('opportunities.title_field'),
            render: (opp) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-full bg-purple-100 dark:bg-purple-900/30 text-purple-600 flex items-center justify-center">
                        <TrendingUp className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-medium">{opp.title}</p>
                        {opp.partnerName && (
                            <div className="flex items-center gap-1 text-sm text-[hsl(var(--muted-foreground))]">
                                <Building2 className="w-3 h-3" />
                                <span>{opp.partnerName}</span>
                            </div>
                        )}
                    </div>
                </div>
            ),
        },
        {
            key: 'value',
            header: t('opportunities.value'),
            render: (opp) => (
                <div className="font-medium">
                    {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(opp.estimatedValue)}
                    <div className="text-xs text-[hsl(var(--muted-foreground))]">
                        {t('opportunities.weightedValue')}: {new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(opp.weightedValue)}
                    </div>
                </div>
            ),
        },
        {
            key: 'stage',
            header: t('opportunities.stage'),
            render: (opp) => (
                <div className="flex flex-col items-start gap-1">
                    {getStageBadge(opp.stage)}
                    <span className="text-xs text-[hsl(var(--muted-foreground))]">{opp.probability}%</span>
                </div>
            ),
        },
        {
            key: 'date',
            header: t('opportunities.expectedCloseDate'),
            render: (opp) => (
                <div className="text-sm">
                    {opp.expectedCloseDate ? new Date(opp.expectedCloseDate).toLocaleDateString() : '-'}
                </div>
            )
        }
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('opportunities.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('opportunities.subtitle')}
                    </p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('opportunities.createOpportunity')}
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={opportunities}
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
                actions={(opp) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => openEditModal(opp)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>

                        <button
                            onClick={() => handleDelete(opp)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

        </div >
    );
}
