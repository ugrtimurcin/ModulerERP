import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Play, CheckCircle, XCircle, Eye, Trash2, Factory } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Select, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface ProductionOrder {
    id: string;
    orderNumber: string;
    productId: string;
    productName?: string;
    bomId: string;
    bomName?: string;
    status: number;
    statusName: string;
    plannedQuantity: number;
    producedQuantity: number;
    plannedStartDate?: string;
    plannedEndDate?: string;
    priority: number;
    createdAt: string;
}

interface Bom {
    id: string;
    code: string;
    name: string;
    productId: string;
}

interface Product {
    id: string;
    name: string;
}

interface Warehouse {
    id: string;
    name: string;
}

const STATUSES = [
    { value: 0, label: 'Draft', variant: 'default' as const },
    { value: 1, label: 'Planned', variant: 'info' as const },
    { value: 2, label: 'Released', variant: 'warning' as const },
    { value: 3, label: 'In Progress', variant: 'info' as const },
    { value: 4, label: 'Completed', variant: 'success' as const },
    { value: 5, label: 'Cancelled', variant: 'error' as const },
];

export function ProductionOrdersPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [orders, setOrders] = useState<ProductionOrder[]>([]);
    const [boms, setBoms] = useState<Bom[]>([]);
    const [products, setProducts] = useState<Product[]>([]);
    const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;
    const [statusFilter, setStatusFilter] = useState<number | undefined>();

    // Create Modal
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [formData, setFormData] = useState({
        orderNumber: '',
        bomId: '',
        productId: '',
        plannedQuantity: 1,
        warehouseId: '',
        priority: 5,
    });

    // Detail Modal
    const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
    const [selectedOrder, setSelectedOrder] = useState<ProductionOrder | null>(null);

    // Record Production Modal
    const [isRecordModalOpen, setIsRecordModalOpen] = useState(false);
    const [recordQuantity, setRecordQuantity] = useState(0);

    const fetchOrders = useCallback(async () => {
        setIsLoading(true);
        const result = await api.productionOrders.getAll(page, pageSize, statusFilter);
        if (result.success && result.data) {
            setOrders(result.data.data || []);
            setTotal(result.data.totalCount || 0);
        }
        setIsLoading(false);
    }, [page, statusFilter]);

    const fetchData = async () => {
        const [bomRes, prodRes, whRes] = await Promise.all([
            api.bom.getAll(1, 100),
            api.products.getAll(),
            api.warehouses.getAll(),
        ]);
        if (bomRes.success && bomRes.data?.data) setBoms(bomRes.data.data);
        if (prodRes.success && prodRes.data) setProducts(prodRes.data);
        if (whRes.success && whRes.data) setWarehouses(whRes.data);
    };

    useEffect(() => {
        fetchOrders();
        fetchData();
    }, [fetchOrders]);

    const handleCreate = async () => {
        const result = await api.productionOrders.create(formData);
        if (result.success) {
            toast.success(t('manufacturing.orders.created'));
            setIsModalOpen(false);
            setFormData({ orderNumber: '', bomId: '', productId: '', plannedQuantity: 1, warehouseId: '', priority: 5 });
            fetchOrders();
        } else {
            toast.error(t('common.error'), result.error);
        }
    };

    const handleDelete = async (order: ProductionOrder) => {
        const confirmed = await dialog.danger({
            title: t('common.confirmDelete'),
            message: t('manufacturing.orders.deleteConfirm', { number: order.orderNumber }),
            confirmText: t('common.delete'),
        });
        if (confirmed) {
            const result = await api.productionOrders.delete(order.id);
            if (result.success) {
                toast.success(t('manufacturing.orders.deleted'));
                fetchOrders();
            }
        }
    };

    const handleViewDetails = async (order: ProductionOrder) => {
        const result = await api.productionOrders.getById(order.id);
        if (result.success && result.data) {
            setSelectedOrder(result.data);
            setIsDetailModalOpen(true);
        }
    };

    const handleLifecycle = async (action: 'plan' | 'release' | 'start' | 'complete' | 'cancel') => {
        if (!selectedOrder) return;
        const actions = {
            plan: api.productionOrders.plan,
            release: api.productionOrders.release,
            start: api.productionOrders.start,
            complete: api.productionOrders.complete,
            cancel: api.productionOrders.cancel,
        };
        const result = await actions[action](selectedOrder.id);
        if (result.success) {
            toast.success(t(`manufacturing.orders.${action}d`));
            setSelectedOrder(result.data);
            fetchOrders();
        }
    };

    const handleRecordProduction = async () => {
        if (!selectedOrder) return;
        const result = await api.productionOrders.recordProduction(selectedOrder.id, recordQuantity);
        if (result.success) {
            toast.success(t('manufacturing.orders.productionRecorded'));
            setIsRecordModalOpen(false);
            setSelectedOrder(result.data);
            fetchOrders();
        }
    };

    const handleBomChange = (bomId: string) => {
        const bom = boms.find(b => b.id === bomId);
        setFormData({ ...formData, bomId, productId: bom?.productId || '' });
    };

    const getStatusBadge = (status: number) => {
        const s = STATUSES.find(st => st.value === status);
        return <Badge variant={s?.variant || 'default'}>{s?.label || status}</Badge>;
    };

    const columns: Column<ProductionOrder>[] = [
        { key: 'orderNumber', header: t('manufacturing.orders.orderNumber') },
        {
            key: 'productId',
            header: t('manufacturing.orders.product'),
            render: (o) => products.find(p => p.id === o.productId)?.name || '-',
        },
        { key: 'status', header: t('manufacturing.orders.status'), render: (o) => getStatusBadge(o.status) },
        {
            key: 'plannedQuantity',
            header: t('manufacturing.orders.progress'),
            render: (o) => `${o.producedQuantity} / ${o.plannedQuantity}`,
        },
        { key: 'priority', header: t('manufacturing.orders.priority') },
        {
            key: 'plannedStartDate',
            header: t('manufacturing.orders.plannedStart'),
            render: (o) => o.plannedStartDate ? new Date(o.plannedStartDate).toLocaleDateString() : '-',
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Factory className="w-7 h-7" />
                        {t('manufacturing.orders.title')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('manufacturing.orders.subtitle')}</p>
                </div>
                <Button onClick={() => setIsModalOpen(true)}>
                    <Plus className="w-4 h-4" />
                    {t('manufacturing.orders.create')}
                </Button>
            </div>

            <div className="flex gap-4 mb-4">
                <Select
                    value={statusFilter?.toString() || ''}
                    onChange={(e) => setStatusFilter(e.target.value ? Number(e.target.value) : undefined)}
                    options={[{ value: '', label: t('common.all') }, ...STATUSES.map(s => ({ value: s.value.toString(), label: s.label }))]}
                    className="w-40"
                />
            </div>

            <DataTable
                data={orders}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                pagination={{ page, pageSize, total, onPageChange: setPage }}
                actions={(order) => (
                    <div className="flex gap-2">
                        <button onClick={() => handleViewDetails(order)} className="p-2 rounded-lg hover:bg-blue-100 dark:hover:bg-blue-900/30 text-blue-600">
                            <Eye className="w-4 h-4" />
                        </button>
                        <button onClick={() => handleDelete(order)} className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600">
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            {/* Create Modal */}
            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={t('manufacturing.orders.create')}
                size="md"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleCreate}>{t('common.save')}</Button>
                    </>
                }
            >
                <div className="space-y-4">
                    <Input label={t('manufacturing.orders.orderNumber')} value={formData.orderNumber} onChange={(e) => setFormData({ ...formData, orderNumber: e.target.value })} required />
                    <Select
                        label={t('manufacturing.orders.bom')}
                        value={formData.bomId}
                        onChange={(e) => handleBomChange(e.target.value)}
                        options={boms.map(b => ({ value: b.id, label: `${b.code} - ${b.name}` }))}
                    />
                    <Input label={t('manufacturing.orders.quantity')} type="number" value={formData.plannedQuantity} onChange={(e) => setFormData({ ...formData, plannedQuantity: Number(e.target.value) })} required />
                    <Select
                        label={t('manufacturing.orders.warehouse')}
                        value={formData.warehouseId}
                        onChange={(e) => setFormData({ ...formData, warehouseId: e.target.value })}
                        options={warehouses.map(w => ({ value: w.id, label: w.name }))}
                    />
                    <Input label={t('manufacturing.orders.priority')} type="number" min={1} max={10} value={formData.priority} onChange={(e) => setFormData({ ...formData, priority: Number(e.target.value) })} />
                </div>
            </Modal>

            {/* Detail Modal */}
            <Modal isOpen={isDetailModalOpen} onClose={() => setIsDetailModalOpen(false)} title={selectedOrder?.orderNumber || ''} size="lg">
                <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4 text-sm">
                        <div><span className="font-medium">{t('manufacturing.orders.status')}:</span> {getStatusBadge(selectedOrder?.status || 0)}</div>
                        <div><span className="font-medium">{t('manufacturing.orders.priority')}:</span> {selectedOrder?.priority}</div>
                        <div><span className="font-medium">{t('manufacturing.orders.planned')}:</span> {selectedOrder?.plannedQuantity}</div>
                        <div><span className="font-medium">{t('manufacturing.orders.produced')}:</span> {selectedOrder?.producedQuantity}</div>
                    </div>

                    {/* Progress Bar */}
                    <div className="w-full bg-[hsl(var(--accent))] rounded-full h-4">
                        <div
                            className="bg-blue-600 h-4 rounded-full transition-all"
                            style={{ width: `${selectedOrder ? (selectedOrder.producedQuantity / selectedOrder.plannedQuantity) * 100 : 0}%` }}
                        ></div>
                    </div>

                    {/* Actions */}
                    <div className="flex flex-wrap gap-2 border-t pt-4">
                        {selectedOrder?.status === 0 && (
                            <Button size="sm" onClick={() => handleLifecycle('plan')}>
                                {t('manufacturing.orders.plan')}
                            </Button>
                        )}
                        {selectedOrder?.status === 1 && (
                            <Button size="sm" onClick={() => handleLifecycle('release')}>
                                {t('manufacturing.orders.release')}
                            </Button>
                        )}
                        {selectedOrder?.status === 2 && (
                            <Button size="sm" onClick={() => handleLifecycle('start')}>
                                <Play className="w-3 h-3" />
                                {t('manufacturing.orders.start')}
                            </Button>
                        )}
                        {selectedOrder?.status === 3 && (
                            <>
                                <Button size="sm" variant="secondary" onClick={() => { setRecordQuantity(0); setIsRecordModalOpen(true); }}>
                                    {t('manufacturing.orders.recordProduction')}
                                </Button>
                                <Button size="sm" onClick={() => handleLifecycle('complete')}>
                                    <CheckCircle className="w-3 h-3" />
                                    {t('manufacturing.orders.complete')}
                                </Button>
                            </>
                        )}
                        {selectedOrder && selectedOrder.status < 4 && (
                            <Button size="sm" variant="secondary" onClick={() => handleLifecycle('cancel')}>
                                <XCircle className="w-3 h-3" />
                                {t('manufacturing.orders.cancel')}
                            </Button>
                        )}
                    </div>
                </div>
            </Modal>

            {/* Record Production Modal */}
            <Modal
                isOpen={isRecordModalOpen}
                onClose={() => setIsRecordModalOpen(false)}
                title={t('manufacturing.orders.recordProduction')}
                size="md"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsRecordModalOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleRecordProduction}>{t('common.save')}</Button>
                    </>
                }
            >
                <div className="space-y-4">
                    <Input
                        label={t('manufacturing.orders.quantity')}
                        type="number"
                        value={recordQuantity}
                        onChange={(e) => setRecordQuantity(Number(e.target.value))}
                    />
                </div>
            </Modal>
        </div>
    );
}
