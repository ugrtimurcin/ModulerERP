import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Eye, ClipboardCheck, Package, Truck } from 'lucide-react';
import { DataTable, Button, Badge, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';

interface GoodsReceipt {
    id: string;
    receiptNumber: string;
    purchaseOrderNumber: string;
    supplierName: string;
    receiptDate: string;
    status: number;
    itemCount: number;
}

const API_BASE = '/api/procurement';

export function GoodsReceiptsPage() {
    const { t } = useTranslation();
    const toast = useToast();

    const [receipts, setReceipts] = useState<GoodsReceipt[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const res = await fetch(`${API_BASE}/goods-receipts`, { cache: 'no-store' });
            if (res.ok) setReceipts(await res.json());
        } catch {
            toast.error('Failed to load goods receipts');
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const getStatusBadge = (status: number) => {
        const configs: Record<number, { variant: 'success' | 'warning' | 'info' | 'default', label: string }> = {
            0: { variant: 'default', label: 'Draft' },
            1: { variant: 'info', label: 'Pending QC' },
            2: { variant: 'success', label: 'Completed' },
        };
        const cfg = configs[status] || configs[0];
        return <Badge variant={cfg.variant}>{cfg.label}</Badge>;
    };

    const columns: Column<GoodsReceipt>[] = [
        {
            key: 'receiptNumber',
            header: t('procurement.receiptNumber'),
            render: (gr) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-green-500 to-emerald-500 flex items-center justify-center text-white">
                        <Package className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-mono font-semibold">{gr.receiptNumber}</p>
                        <p className="text-xs text-[hsl(var(--muted-foreground))]">
                            PO: {gr.purchaseOrderNumber}
                        </p>
                    </div>
                </div>
            ),
        },
        {
            key: 'supplierName',
            header: t('procurement.supplier'),
            render: (gr) => <span className="font-medium">{gr.supplierName}</span>,
        },
        {
            key: 'receiptDate',
            header: t('procurement.receiptDate'),
            render: (gr) => <span className="text-sm">{new Date(gr.receiptDate).toLocaleDateString()}</span>,
        },
        {
            key: 'itemCount',
            header: t('procurement.items'),
            render: (gr) => (
                <span className="font-mono">{gr.itemCount}</span>
            ),
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (gr) => getStatusBadge(gr.status),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Truck className="w-6 h-6" />
                        {t('procurement.goodsReceipts')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('procurement.goodsReceiptsSubtitle')}</p>
                </div>
                <Button onClick={() => { }}>
                    <Plus className="w-4 h-4" />
                    {t('procurement.createGRN')}
                </Button>
            </div>

            <DataTable
                data={receipts}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(gr) => (
                    <div className="flex items-center gap-1">
                        <button onClick={() => { }} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]" title={t('common.view')}>
                            <Eye className="w-4 h-4" />
                        </button>
                        {gr.status === 1 && (
                            <button onClick={() => { }} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] text-green-600" title="QC">
                                <ClipboardCheck className="w-4 h-4" />
                            </button>
                        )}
                    </div>
                )}
            />
        </div>
    );
}
