import { useState, useEffect } from 'react';
import { api } from '@/services/api';
import { useToast, Button, Modal, Input, Select, Badge } from '@/components/ui';
import { useTranslation } from 'react-i18next';
import {
    Plus,
    ArrowRightLeft,
    RefreshCw
} from 'lucide-react';

interface StockMovement {
    id: string;
    productName: string;
    productSku: string;
    warehouseName: string;
    type: number;
    quantity: number;
    referenceType: string;
    referenceNumber: string;
    movementDate: string;
    createdBy: string;
}

export default function StockMovementsPage() {
    const { t } = useTranslation();
    const { success, error: toastError } = useToast();
    const [loading, setLoading] = useState(false);
    const [movements, setMovements] = useState<StockMovement[]>([]);

    // Filters
    const [warehouses, setWarehouses] = useState<any[]>([]);
    const [selectedWarehouse, setSelectedWarehouse] = useState<string>('');
    const [fromDate, setFromDate] = useState('');
    const [toDate, setToDate] = useState('');

    // Modals
    const [showMovementModal, setShowMovementModal] = useState(false);
    const [showTransferModal, setShowTransferModal] = useState(false);

    useEffect(() => {
        loadWarehouses();
        loadMovements();
    }, []);

    useEffect(() => {
        loadMovements();
    }, [selectedWarehouse, fromDate, toDate]);

    const loadWarehouses = async () => {
        const response = await api.warehouses.getAll();
        if (response.success && response.data) {
            setWarehouses(response.data);
        }
    };

    const loadMovements = async () => {
        setLoading(true);
        try {
            const params: any = {};
            if (selectedWarehouse) params.warehouseId = selectedWarehouse;
            if (fromDate) params.fromDate = new Date(fromDate).toISOString();
            if (toDate) params.toDate = new Date(toDate).toISOString();

            const response = await api.stock.getMovements(params);
            if (response.success && response.data) {
                setMovements(response.data);
            }
        } catch (err) {
            toastError('Failed to load movements');
        } finally {
            setLoading(false);
        }
    };

    const getMovementTypeLabel = (type: number) => {
        const keys: Record<number, string> = {
            1: 'inventory.movementTypes.purchase',
            2: 'inventory.movementTypes.sale',
            3: 'inventory.movementTypes.transfer',
            4: 'inventory.movementTypes.adjustmentIn',
            5: 'inventory.movementTypes.adjustmentOut',
            6: 'inventory.movementTypes.consumption',
            7: 'inventory.movementTypes.production',
            8: 'inventory.movementTypes.salesReturn',
            9: 'inventory.movementTypes.purchaseReturn'
        };
        return keys[type] ? t(keys[type]) : `Type ${type}`;
    };

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold text-gray-900">{t('inventory.stockMovements', 'Stock Movements')}</h1>
                    <p className="mt-1 text-sm text-gray-500">
                        {t('inventory.stockMovementsDesc', 'History of all stock transactions')}
                    </p>
                </div>
                <div className="flex space-x-3">
                    <Button onClick={() => setShowMovementModal(true)}>
                        <Plus className="w-4 h-4 mr-2" />
                        {t('inventory.newMovement', 'New Movement')}
                    </Button>
                    <Button variant="secondary" onClick={() => setShowTransferModal(true)}>
                        <ArrowRightLeft className="w-4 h-4 mr-2" />
                        {t('inventory.newTransfer', 'Transfer')}
                    </Button>
                </div>
            </div>

            {/* Filters */}
            <div className="bg-white p-4 rounded-lg shadow-sm border border-gray-200 flex flex-wrap gap-4 items-center">
                <div className="w-64">
                    <Select
                        value={selectedWarehouse}
                        onChange={(e) => setSelectedWarehouse(e.target.value)}
                        options={[
                            { value: '', label: t('inventory.allWarehouses', 'All Warehouses') },
                            ...warehouses.map(w => ({ value: w.id, label: w.name }))
                        ]}
                    />
                </div>
                <div className="flex items-center space-x-2">
                    <span className="text-sm text-gray-500">From:</span>
                    <Input
                        type="date"
                        value={fromDate}
                        onChange={e => setFromDate(e.target.value)}
                    />
                </div>
                <div className="flex items-center space-x-2">
                    <span className="text-sm text-gray-500">To:</span>
                    <Input
                        type="date"
                        value={toDate}
                        onChange={e => setToDate(e.target.value)}
                    />
                </div>
                <Button variant="ghost" onClick={() => loadMovements()}>
                    <RefreshCw className="w-4 h-4" />
                </Button>
            </div>

            {/* Table */}
            <div className="bg-white rounded-lg shadow overflow-hidden">
                <div className="overflow-x-auto">
                    <table className="min-w-full divide-y divide-gray-200">
                        <thead className="bg-gray-50">
                            <tr>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Product</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Warehouse</th>
                                <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Quantity</th>
                                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Reference</th>
                            </tr>
                        </thead>
                        <tbody className="bg-white divide-y divide-gray-200">
                            {loading ? (
                                <tr>
                                    <td colSpan={6} className="px-6 py-12 text-center text-sm text-gray-500">
                                        <div className="flex flex-col items-center justify-center gap-2">
                                            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
                                            <span>Loading movements...</span>
                                        </div>
                                    </td>
                                </tr>
                            ) : movements.length === 0 ? (
                                <tr>
                                    <td colSpan={6} className="px-6 py-4 text-center text-sm text-gray-500">
                                        No movements found
                                    </td>
                                </tr>
                            ) : (
                                movements.map((m) => (
                                    <tr key={m.id} className="hover:bg-gray-50">
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            {new Date(m.movementDate).toLocaleDateString()} {new Date(m.movementDate).toLocaleTimeString()}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <Badge variant={m.quantity > 0 ? 'success' : 'error'}>
                                                {getMovementTypeLabel(m.type)}
                                            </Badge>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap">
                                            <div className="text-sm font-medium text-gray-900">{m.productName}</div>
                                            <div className="text-sm text-gray-500">{m.productSku}</div>
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            {m.warehouseName}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-right font-bold">
                                            {m.quantity > 0 ? '+' : ''}{m.quantity}
                                        </td>
                                        <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">
                                            {m.referenceNumber && (
                                                <span className="bg-gray-100 text-gray-600 py-0.5 px-2 rounded text-xs">
                                                    {m.referenceType}: {m.referenceNumber}
                                                </span>
                                            )}
                                        </td>
                                    </tr>
                                )))}
                        </tbody>
                    </table>
                </div>
            </div>

            {/* Movement Modal */}
            <MovementFormModal
                isOpen={showMovementModal}
                onClose={() => setShowMovementModal(false)}
                onSuccess={() => {
                    setShowMovementModal(false);
                    loadMovements();
                    success('Movement created successfully');
                }}
                warehouses={warehouses}
            />

            {/* Transfer Modal */}
            <TransferFormModal
                isOpen={showTransferModal}
                onClose={() => setShowTransferModal(false)}
                onSuccess={() => {
                    setShowTransferModal(false);
                    loadMovements();
                    success('Transfer created successfully');
                }}
                warehouses={warehouses}
            />
        </div>
    );
}

// Sub-components for Modals
function MovementFormModal({ isOpen, onClose, onSuccess, warehouses }: any) {
    const { t } = useTranslation();
    const { error: toastError } = useToast();
    const [products, setProducts] = useState<any[]>([]);
    const [formData, setFormData] = useState({
        productId: '',
        warehouseId: '',
        type: '4', // Adjustment In default
        quantity: '',
        notes: '',
        referenceNumber: ''
    });

    useEffect(() => {
        api.products.getAll().then(res => {
            if (res.success && res.data) setProducts(res.data);
        });
    }, []);

    const handleSubmit = async () => {
        try {
            const payload = {
                ...formData,
                type: parseInt(formData.type),
                quantity: parseFloat(formData.quantity),
                movementDate: new Date().toISOString()
            };
            const res = await api.stock.createMovement(payload);
            if (res.success) onSuccess();
            else toastError(res.error || t('common.error'));
        } catch (err) {
            toastError(t('common.error'));
        }
    };

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={t('inventory.newMovement')}
            footer={
                <>
                    <Button variant="secondary" onClick={onClose}>{t('common.cancel')}</Button>
                    <Button onClick={handleSubmit}>{t('common.save')}</Button>
                </>
            }
        >
            <div className="space-y-4">
                <Select
                    label={t('inventory.type')}
                    value={formData.type}
                    onChange={e => setFormData({ ...formData, type: e.target.value })}
                    options={[
                        { value: '4', label: t('inventory.movementTypes.adjustmentIn') },
                        { value: '5', label: t('inventory.movementTypes.adjustmentOut') },
                        { value: '1', label: t('inventory.movementTypes.purchase') },
                        { value: '6', label: t('inventory.movementTypes.consumption') }
                    ]}
                />
                <Select
                    label={t('inventory.product')}
                    value={formData.productId}
                    onChange={e => setFormData({ ...formData, productId: e.target.value })}
                    options={[
                        { value: '', label: t('common.select') },
                        ...products.map(p => ({ value: p.id, label: `${p.sku} - ${p.name}` }))
                    ]}
                    required
                />
                <Select
                    label={t('inventory.warehouse')}
                    value={formData.warehouseId}
                    onChange={e => setFormData({ ...formData, warehouseId: e.target.value })}
                    options={[
                        { value: '', label: t('common.select') },
                        ...warehouses.map((w: any) => ({ value: w.id, label: w.name }))
                    ]}
                    required
                />
                <Input
                    label={t('inventory.prices.quantity') || 'Quantity'}
                    type="number"
                    value={formData.quantity}
                    onChange={e => setFormData({ ...formData, quantity: e.target.value })}
                    required
                    min={0.0001}
                    step="any"
                />
                <Input
                    label={t('inventory.reference') || 'Reference'}
                    value={formData.referenceNumber}
                    onChange={e => setFormData({ ...formData, referenceNumber: e.target.value })}
                    placeholder={t('common.optional') || 'Optional'}
                />
            </div>
        </Modal>
    );
}

function TransferFormModal({ isOpen, onClose, onSuccess, warehouses }: any) {
    const { t } = useTranslation();
    const { error: toastError } = useToast();
    const [products, setProducts] = useState<any[]>([]);
    const [formData, setFormData] = useState({
        productId: '',
        sourceWarehouseId: '',
        targetWarehouseId: '',
        quantity: '',
        notes: '',
        referenceNumber: ''
    });

    useEffect(() => {
        api.products.getAll().then(res => {
            if (res.success && res.data) setProducts(res.data);
        });
    }, []);

    const handleSubmit = async () => {
        try {
            const payload = {
                ...formData,
                quantity: parseFloat(formData.quantity),
                transferDate: new Date().toISOString()
            };
            const res = await api.stock.createTransfer(payload);
            if (res.success) onSuccess();
            else toastError(res.error || t('common.error'));
        } catch (err) {
            toastError(t('common.error'));
        }
    };

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={t('inventory.newTransfer')}
            footer={
                <>
                    <Button variant="secondary" onClick={onClose}>{t('common.cancel')}</Button>
                    <Button onClick={handleSubmit}>{t('common.create')}</Button>
                </>
            }
        >
            <div className="space-y-4">
                <Select
                    label={t('inventory.product')}
                    value={formData.productId}
                    onChange={e => setFormData({ ...formData, productId: e.target.value })}
                    options={[
                        { value: '', label: t('common.select') },
                        ...products.map(p => ({ value: p.id, label: `${p.sku} - ${p.name}` }))
                    ]}
                    required
                />
                <div className="grid grid-cols-2 gap-4">
                    <Select
                        label={t('common.from') || 'From'}
                        value={formData.sourceWarehouseId}
                        onChange={e => setFormData({ ...formData, sourceWarehouseId: e.target.value })}
                        options={[
                            { value: '', label: t('common.select') },
                            ...warehouses.map((w: any) => ({ value: w.id, label: w.name }))
                        ]}
                        required
                    />
                    <Select
                        label={t('common.to') || 'To'}
                        value={formData.targetWarehouseId}
                        onChange={e => setFormData({ ...formData, targetWarehouseId: e.target.value })}
                        options={[
                            { value: '', label: t('common.select') },
                            ...warehouses.filter((w: any) => w.id !== formData.sourceWarehouseId).map((w: any) => ({ value: w.id, label: w.name }))
                        ]}
                        required
                    />
                </div>
                <Input
                    label={t('inventory.prices.quantity') || 'Quantity'}
                    type="number"
                    value={formData.quantity}
                    onChange={e => setFormData({ ...formData, quantity: e.target.value })}
                    required
                    min={0.0001}
                    step="any"
                />
                <Input
                    label={t('inventory.reference') || 'Reference'}
                    value={formData.referenceNumber}
                    onChange={e => setFormData({ ...formData, referenceNumber: e.target.value })}
                    placeholder={t('common.optional') || 'Optional'}
                />
            </div>
        </Modal>
    );
}
