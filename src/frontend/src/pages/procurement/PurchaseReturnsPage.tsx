import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Reply, Eye } from 'lucide-react';
import { DataTable, Button, Badge, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';
import { api } from '@/services/api';
import { PurchaseReturnStatus } from '@/types/procurement';
import { PurchaseReturnDialog } from './PurchaseReturnDialog';

export function PurchaseReturnsPage() {
    const { t } = useTranslation();
    const toast = useToast();

    const [returns, setReturns] = useState<any[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [isDialogOpen, setIsDialogOpen] = useState(false);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const res = await api.returns.getAll();
            if (Array.isArray(res)) {
                setReturns(res);
            } else if (res && (res as any).data && Array.isArray((res as any).data)) {
                setReturns((res as any).data);
            }
        } catch {
            setReturns([]);
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const getStatusBadge = (status: number) => {
        switch (status) {
            case PurchaseReturnStatus.Draft:
                return <Badge variant="default">{t('common.draft')}</Badge>;
            case PurchaseReturnStatus.Shipped:
                return <Badge variant="info">{t('procurement.status.shipped')}</Badge>;
            case PurchaseReturnStatus.Completed:
                return <Badge variant="success">{t('common.completed')}</Badge>;
            case PurchaseReturnStatus.Cancelled:
                return <Badge variant="default">{t('common.cancelled')}</Badge>;
            default:
                return <Badge variant="default">{t('common.unknown')}</Badge>;
        }
    };

    const columns: Column<any>[] = [
        {
            key: 'returnNumber',
            header: t('procurement.returnNumber'),
            render: (ret) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-red-500 to-rose-500 flex items-center justify-center text-white">
                        <Reply className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-mono font-semibold">{ret.returnNumber}</p>
                        <p className="text-xs text-[hsl(var(--muted-foreground))]">
                            {new Date(ret.createdAt || new Date()).toLocaleDateString()}
                        </p>
                    </div>
                </div>
            ),
        },
        {
            key: 'supplier',
            header: t('procurement.supplier'),
            render: (ret) => <span className="font-medium">{ret.supplierName || 'N/A'}</span>,
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (ret) => getStatusBadge(ret.status),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Reply className="w-6 h-6" />
                        {t('procurement.purchaseReturns')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('procurement.purchaseReturnsSubtitle')}</p>
                </div>
                <Button onClick={() => setIsDialogOpen(true)}>
                    <Plus className="w-4 h-4" />
                    {t('procurement.createReturn')}
                </Button>
            </div>

            <DataTable
                data={returns}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(ret) => (
                    <div className="flex items-center gap-1">
                        <button className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]" title={t('common.view')} onClick={() => console.log(ret)}>
                            <Eye className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <PurchaseReturnDialog
                open={isDialogOpen}
                onClose={(saved) => {
                    setIsDialogOpen(false);
                    if (saved) loadData();
                }}
                ret={null}
            />
        </div>
    );
}
