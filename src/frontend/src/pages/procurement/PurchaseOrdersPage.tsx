import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Eye, ShoppingCart } from 'lucide-react';
import { DataTable, Button, Badge, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';
import { PurchaseOrderDialog } from './PurchaseOrderDialog';

interface PurchaseOrder {
    id: string;
    orderNumber: string;
    supplierId: string;
    supplierName: string;
    status: number;
    currencyCode: string;
    totalAmount: number;
    orderDate: string;
    expectedDate: string | null;
}

const API_BASE = '/api/procurement';

export function PurchaseOrdersPage() {
    const { t } = useTranslation();
    const toast = useToast();

    const [orders, setOrders] = useState<PurchaseOrder[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [editingOrder, setEditingOrder] = useState<PurchaseOrder | null>(null);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const res = await fetch(`${API_BASE}/purchase-orders`, { cache: 'no-store' });
            if (res.ok) setOrders(await res.json());
        } catch {
            toast.error('Failed to load purchase orders');
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => { loadData(); }, [loadData]);

    const handleCreate = () => { setEditingOrder(null); setDialogOpen(true); };
    const handleEdit = (order: PurchaseOrder) => { setEditingOrder(order); setDialogOpen(true); };
    const handleDialogClose = (saved: boolean) => { setDialogOpen(false); if (saved) loadData(); };

    const getStatusBadge = (status: number) => {
        const configs: Record<number, { variant: 'success' | 'warning' | 'info' | 'default', label: string }> = {
            0: { variant: 'default', label: 'Draft' },
            1: { variant: 'info', label: 'Sent' },
            2: { variant: 'warning', label: 'Confirmed' },
            3: { variant: 'success', label: 'Received' },
            4: { variant: 'success', label: 'Closed' },
            5: { variant: 'default', label: 'Cancelled' },
        };
        const cfg = configs[status] || configs[0];
        return <Badge variant={cfg.variant}>{cfg.label}</Badge>;
    };

    const columns: Column<PurchaseOrder>[] = [
        {
            key: 'orderNumber',
            header: t('procurement.orderNumber'),
            render: (po) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-lg bg-gradient-to-br from-orange-500 to-amber-500 flex items-center justify-center text-white">
                        <ShoppingCart className="w-5 h-5" />
                    </div>
                    <div>
                        <p className="font-mono font-semibold">{po.orderNumber}</p>
                        <p className="text-xs text-[hsl(var(--muted-foreground))]">
                            {new Date(po.orderDate).toLocaleDateString()}
                        </p>
                    </div>
                </div>
            ),
        },
        {
            key: 'supplierName',
            header: t('procurement.supplier'),
            render: (po) => <span className="font-medium">{po.supplierName}</span>,
        },
        {
            key: 'totalAmount',
            header: t('common.amount'),
            render: (po) => (
                <span className="font-mono font-medium">
                    {po.currencyCode} {po.totalAmount?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                </span>
            ),
        },
        {
            key: 'expectedDate',
            header: t('procurement.expectedDate'),
            render: (po) => po.expectedDate ? (
                <span className="text-sm">{new Date(po.expectedDate).toLocaleDateString()}</span>
            ) : <span className="text-[hsl(var(--muted-foreground))]">—</span>,
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (po) => getStatusBadge(po.status),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <ShoppingCart className="w-6 h-6" />
                        {t('procurement.purchaseOrders')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('procurement.purchaseOrdersSubtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4" />
                    {t('procurement.createPO')}
                </Button>
            </div>

            <DataTable
                data={orders}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(po) => (
                    <div className="flex items-center gap-1">
                        <button onClick={() => handleEdit(po)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]" title={t('common.view')}>
                            <Eye className="w-4 h-4" />
                        </button>
                        <button onClick={() => handleEdit(po)} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]" title={t('common.edit')}>
                            <Pencil className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <PurchaseOrderDialog open={dialogOpen} onClose={handleDialogClose} order={editingOrder} />
        </div>
    );
}
