import React, { useEffect, useState, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Check, X, FileText } from 'lucide-react';
import { api } from '../../services/api';
import { DataTable, Button, Badge, useDialog, useToast } from '@/components/ui';
import type { Column } from '@/components/ui';
import OrderDialog from './OrderDialog';

interface Order {
    id: string;
    orderNumber: string;
    partnerName: string;
    partnerId: string;
    createdAt: string;
    status: string;
    totalAmount: number;
    currencyCode: string;
}

const OrdersPage: React.FC = () => {
    const { t } = useTranslation();
    const dialog = useDialog();
    const toast = useToast();

    const [orders, setOrders] = useState<Order[]>([]);
    const [loading, setLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    // Dialog state
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [selectedOrderId, setSelectedOrderId] = useState<string | undefined>(undefined);

    const loadOrders = useCallback(async () => {
        setLoading(true);
        try {
            const response = await api.orders.getAll(page, pageSize);
            if (response.success && response.data) {
                setOrders(response.data.data);
                setTotal(response.data.totalCount);
            }
        } catch (error) {
            console.error('Failed to load orders', error);
            toast.error(t('common.error'), 'Failed to load orders');
        } finally {
            setLoading(false);
        }
    }, [page, toast]);

    useEffect(() => {
        loadOrders();
    }, [loadOrders]);

    const handleCreate = () => {
        setSelectedOrderId(undefined);
        setIsDialogOpen(true);
    };

    const handleEdit = (order: Order) => {
        setSelectedOrderId(order.id);
        setIsDialogOpen(true);
    };

    const handleDelete = async (order: Order) => {
        const confirmed = await dialog.danger({
            title: t('common.delete'),
            message: t('common.confirmDelete'),
            confirmText: t('common.delete'),
            cancelText: t('common.cancel')
        });

        if (!confirmed) return;

        try {
            const response = await api.orders.delete(order.id);
            if (response.success) {
                toast.success(t('common.success'), t('quotes.orders.orderDeleted'));
                loadOrders();
            } else {
                toast.error(t('common.error'), response.error);
            }
        } catch (error) {
            console.error('Failed to delete order', error);
            toast.error(t('common.error'));
        }
    };

    const handleConfirm = async (order: Order) => {
        const confirmed = await dialog.confirm({
            title: t('quotes.orders.confirmOrder'),
            message: t('quotes.orders.confirmOrderMessage'),
            confirmText: t('quotes.orders.confirm')
        });

        if (!confirmed) return;

        try {
            const response = await api.orders.confirm(order.id);
            if (response.success) {
                toast.success(t('common.success'), t('quotes.orders.orderConfirmed'));
                loadOrders();
            } else {
                toast.error(t('common.error'), response.error);
            }
        } catch (error) {
            console.error('Failed to confirm', error);
            toast.error(t('common.error'));
        }
    };

    const handleCancel = async (order: Order) => {
        const confirmed = await dialog.danger({
            title: t('quotes.orders.cancelOrder'),
            message: t('quotes.orders.cancelOrderMessage'),
            confirmText: t('quotes.orders.cancelOrder')
        });

        if (!confirmed) return;

        try {
            const response = await api.orders.cancel(order.id);
            if (response.success) {
                toast.success(t('common.success'), t('quotes.orders.orderCancelled'));
                loadOrders();
            } else {
                toast.error(t('common.error'), response.error);
            }
        } catch (error) {
            console.error('Failed to cancel', error);
            toast.error(t('common.error'));
        }
    };

    const getStatusVariant = (status: string) => {
        switch (status) {
            case 'Pending': return 'warning';
            case 'Confirmed': return 'info';
            case 'Shipped': return 'info';
            case 'Delivered': return 'success';
            case 'Cancelled': return 'default';
            default: return 'default';
        }
    };

    const columns: Column<Order>[] = [
        {
            key: 'orderNumber',
            header: t('quotes.quoteNumber'), // Should be orders.orderNumber
            render: (row) => (
                <div className="flex items-center gap-2">
                    <FileText className="w-4 h-4 text-gray-500" />
                    <span
                        className="font-medium text-blue-600 cursor-pointer hover:underline"
                        onClick={() => handleEdit(row)}
                    >
                        {row.orderNumber}
                    </span>
                </div>
            )
        },
        {
            key: 'partnerName',
            header: t('quotes.partner'),
            render: (row) => row.partnerName || row.partnerId
        },
        {
            key: 'createdAt',
            header: t('quotes.date'),
            render: (row) => new Date(row.createdAt).toLocaleDateString()
        },
        {
            key: 'status',
            header: t('quotes.status'),
            render: (row) => (
                <Badge variant={getStatusVariant(row.status)}>
                    {row.status}
                </Badge>
            )
        },
        {
            key: 'totalAmount',
            header: t('quotes.total'),
            align: 'right',
            render: (row) => `${row.totalAmount.toFixed(2)} ${row.currencyCode}`
        }
    ];

    return (
        <div className="space-y-6">
            {/* Header */}
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('orders.title')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('orders.subtitle')}</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4 mr-2" />
                    {t('orders.createOrder')}
                </Button>
            </div>

            {/* Table */}
            <DataTable
                data={orders}
                columns={columns}
                keyField="id"
                isLoading={loading}
                pagination={{
                    page,
                    pageSize,
                    total,
                    onPageChange: setPage
                }}
                actions={(row) => (
                    <div className="flex items-center gap-1">
                        {row.status === 'Pending' && (
                            <>
                                <button
                                    onClick={() => handleConfirm(row)}
                                    className="p-2 rounded-lg hover:bg-green-100 dark:hover:bg-green-900/30 text-green-600 transition-colors"
                                    title={t('quotes.orders.confirm')}
                                >
                                    <Check className="w-4 h-4" />
                                </button>
                                <button
                                    onClick={() => handleEdit(row)}
                                    className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                                    title={t('quotes.orders.edit')}
                                >
                                    <Pencil className="w-4 h-4" />
                                </button>
                                <button
                                    onClick={() => handleCancel(row)}
                                    className="p-2 rounded-lg hover:bg-orange-100 dark:hover:bg-orange-900/30 text-orange-600 transition-colors"
                                    title={t('quotes.orders.cancel')}
                                >
                                    <X className="w-4 h-4" />
                                </button>
                            </>
                        )}
                        <button
                            onClick={() => handleDelete(row)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('quotes.orders.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <OrderDialog
                isOpen={isDialogOpen}
                onClose={() => setIsDialogOpen(false)}
                onSave={loadOrders}
                orderId={selectedOrderId}
            />
        </div>
    );
};

export default OrdersPage;
