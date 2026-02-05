import React, { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Truck, CheckCircle } from 'lucide-react';
import { api, type PagedResult } from '../../services/api';
import { DataTable, type Column } from '../../components/ui/DataTable';
import { Button, Badge, useToast, useDialog } from '@/components/ui';
import ShipmentDialog from './ShipmentDialog';

export const ShipmentsPage: React.FC = () => {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();
    const [loading, setLoading] = useState(false);
    const [data, setData] = useState<PagedResult<any>>({ data: [], page: 1, pageSize: 20, totalCount: 0, totalPages: 0 });
    const [isDialogOpen, setIsDialogOpen] = useState(false);
    const [selectedId, setSelectedId] = useState<string | undefined>(undefined);

    const loadData = async (page = 1) => {
        setLoading(true);
        try {
            const res = await api.shipments.getAll(page, 20);
            if (res.success && res.data) {
                setData(res.data);
            }
        } catch (error) {
            console.error(error);
            toast.error(t('common.error'), 'Failed to load shipments');
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        loadData();
    }, []);

    const handleCreate = () => {
        setSelectedId(undefined);
        setIsDialogOpen(true);
    };

    const handleEdit = (row: any) => {
        setSelectedId(row.id);
        setIsDialogOpen(true);
    };

    const handleShip = async (row: any) => {
        const confirmed = await dialog.confirm({
            title: t('shipments.ship'),
            message: t('shipments.confirmShip'),
            confirmText: t('shipments.ship')
        });
        if (!confirmed) return;

        try {
            const res = await api.shipments.ship(row.id);
            if (res.success) {
                toast.success(t('common.success'), t('shipments.shippedSuccess'));
                loadData(data.page);
            } else {
                toast.error(t('common.error'), res.error || t('shipments.shipFailed'));
            }
        } catch (error) {
            toast.error(t('common.error'));
        }
    };

    const handleDeliver = async (row: any) => {
        const confirmed = await dialog.confirm({
            title: t('shipments.deliver'),
            message: t('shipments.confirmDeliver'),
            confirmText: t('shipments.deliver')
        });
        if (!confirmed) return;

        try {
            const res = await api.shipments.deliver(row.id);
            if (res.success) {
                toast.success(t('common.success'), t('shipments.deliveredSuccess'));
                loadData(data.page);
            } else {
                toast.error(t('common.error'), res.error || t('shipments.deliverFailed'));
            }
        } catch (error) {
            toast.error(t('common.error'));
        }
    };

    const getStatusVariant = (status: number) => {
        switch (status) {
            case 0: return 'warning'; // Pending
            case 1: return 'info';    // Shipped
            case 2: return 'success'; // Delivered
            default: return 'default';
        }
    };

    const getStatusLabel = (status: number) => {
        switch (status) {
            case 0: return 'Pending';
            case 1: return 'Shipped';
            case 2: return 'Delivered';
            case 3: return 'Failed';
            default: return 'Unknown';
        }
    };

    const columns: Column<any>[] = [
        { header: t('shipments.shipmentNumber'), key: 'shipmentNumber' },
        { header: 'Order', key: 'orderId', render: (row: any) => row.orderNumber || '...' },
        { header: 'Date', key: 'createdAt', render: (row: any) => new Date(row.createdAt).toLocaleDateString() },
        {
            header: t('common.status'),
            key: 'status',
            render: (row: any) => (
                <Badge variant={getStatusVariant(row.status)}>{getStatusLabel(row.status)}</Badge>
            )
        },
        { header: 'Tracking', key: 'trackingNumber' },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">Shipments</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">Manage outbound shipments</p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="mr-2 h-4 w-4" />
                    {t('common.create')}
                </Button>
            </div>

            <DataTable
                columns={columns}
                data={data.data}
                isLoading={loading}
                keyField="id"
                pagination={{
                    page: data.page,
                    pageSize: data.pageSize,
                    total: data.totalCount,
                    onPageChange: loadData
                }}
                actions={(row) => (
                    <div className="flex space-x-2">
                        {row.status === 0 && ( /* Pending */
                            <>
                                <Button variant="ghost" size="sm" onClick={() => handleEdit(row)}>
                                    Edit
                                </Button>
                                <Button variant="secondary" size="sm" onClick={() => handleShip(row)}>
                                    <Truck className="h-4 w-4 mr-1" /> Ship
                                </Button>
                            </>
                        )}
                        {row.status === 1 && ( /* Shipped */
                            <Button variant="secondary" size="sm" onClick={() => handleDeliver(row)}>
                                <CheckCircle className="h-4 w-4 mr-1" /> Deliver
                            </Button>
                        )}
                    </div>
                )}
            />

            <ShipmentDialog
                isOpen={isDialogOpen}
                onClose={() => setIsDialogOpen(false)}
                onSave={() => loadData(data.page)}
                shipmentId={selectedId}
            />
        </div>
    );
};
