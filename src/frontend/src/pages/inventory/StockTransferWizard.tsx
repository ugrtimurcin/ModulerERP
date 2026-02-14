import { useState, useEffect } from 'react';
import { api } from '@/services/api';
import { Button, Input, Select, useToast } from '@/components/ui';
import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { ArrowRight, ArrowLeft, Check, Plus, Trash2 } from 'lucide-react';

export default function StockTransferWizard() {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const { success, error: toastError } = useToast();

    const [step, setStep] = useState(1);
    const [loading, setLoading] = useState(false);
    const [warehouses, setWarehouses] = useState<any[]>([]);

    // Form Data
    const [sourceId, setSourceId] = useState('');
    const [targetId, setTargetId] = useState('');
    const [items, setItems] = useState<{ productId: string, quantity: number, productName?: string, currentStock?: number }[]>([]);
    const [notes, setNotes] = useState('');

    // Product Selection
    const [availableProducts, setAvailableProducts] = useState<any[]>([]);
    const [selectedProduct, setSelectedProduct] = useState('');
    const [qty, setQty] = useState(1);

    useEffect(() => {
        loadWarehouses();
    }, []);

    useEffect(() => {
        if (sourceId) {
            loadSourceProducts(sourceId);
        }
    }, [sourceId]);

    const loadWarehouses = async () => {
        const res = await api.warehouses.getAll();
        if (res.success && res.data) setWarehouses(res.data);
    };

    const loadSourceProducts = async (whId: string) => {
        // Fetch stock levels for source warehouse to pick products
        const res = await api.stock.getLevels(whId);
        if (res.success && res.data) {
            setAvailableProducts(res.data.filter((l: any) => l.quantityAvailable > 0));
        }
    };

    const addItem = () => {
        if (!selectedProduct || qty <= 0) return;

        const product = availableProducts.find(p => p.productId === selectedProduct);
        if (!product) return;

        if (items.some(i => i.productId === selectedProduct)) {
            toastError("Product already added");
            return;
        }

        if (qty > product.quantityAvailable) {
            toastError(`Only ${product.quantityAvailable} available`);
            return;
        }

        setItems([...items, {
            productId: selectedProduct,
            quantity: qty,
            productName: product.productName,
            currentStock: product.quantityAvailable
        }]);

        setSelectedProduct('');
        setQty(1);
    };

    const removeItem = (pid: string) => {
        setItems(items.filter(i => i.productId !== pid));
    };

    const handleSubmit = async () => {
        if (!sourceId || !targetId || items.length === 0) return;

        setLoading(true);
        try {
            const payload = {
                sourceWarehouseId: sourceId,
                destinationWarehouseId: targetId,
                items: items.map(i => ({ productId: i.productId, quantity: i.quantity })),
                notes: notes,
                transferDate: new Date().toISOString()
            };

            const res = await api.stock.createTransfer(payload);
            if (res.success) {
                success("Transfer created successfully");
                navigate('/inventory/transfers');
            } else {
                toastError(res.error || "Failed to create transfer");
            }
        } catch (err) {
            toastError("An error occurred");
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="max-w-4xl mx-auto p-6 bg-white rounded-lg shadow">
            <h1 className="text-2xl font-bold mb-6">{t('inventory.newTransfer', 'New Stock Transfer')}</h1>

            {/* Stepper */}
            <div className="flex mb-8">
                <div className={`flex-1 border-b-2 ${step >= 1 ? 'border-blue-600' : 'border-gray-200'} pb-2 text-center`}>
                    Step 1: Warehouses
                </div>
                <div className={`flex-1 border-b-2 ${step >= 2 ? 'border-blue-600' : 'border-gray-200'} pb-2 text-center`}>
                    Step 2: Items
                </div>
                <div className={`flex-1 border-b-2 ${step >= 3 ? 'border-blue-600' : 'border-gray-200'} pb-2 text-center`}>
                    Step 3: Review
                </div>
            </div>

            {/* Step 1 */}
            {step === 1 && (
                <div className="space-y-4">
                    <div>
                        <label className="block text-sm font-medium mb-1">Source Warehouse</label>
                        <Select
                            value={sourceId}
                            onChange={(e) => setSourceId(e.target.value)}
                            options={warehouses.map(w => ({ value: w.id, label: w.name }))}
                        />
                    </div>
                    <div>
                        <label className="block text-sm font-medium mb-1">Destination Warehouse</label>
                        <Select
                            value={targetId}
                            onChange={(e) => setTargetId(e.target.value)}
                            options={warehouses.filter(w => w.id !== sourceId).map(w => ({ value: w.id, label: w.name }))}
                        />
                    </div>
                    <div className="flex justify-end mt-4">
                        <Button onClick={() => setStep(2)} disabled={!sourceId || !targetId}>
                            Next <ArrowRight className="ml-2 w-4 h-4" />
                        </Button>
                    </div>
                </div>
            )}

            {/* Step 2 */}
            {step === 2 && (
                <div className="space-y-6">
                    <div className="flex gap-4 items-end bg-gray-50 p-4 rounded">
                        <div className="flex-1">
                            <label className="block text-sm font-medium mb-1">Product</label>
                            <Select
                                value={selectedProduct}
                                onChange={(e) => setSelectedProduct(e.target.value)}
                                options={availableProducts.map(p => ({
                                    value: p.productId,
                                    label: `${p.productName} (${p.quantityAvailable})`
                                }))}
                            />
                        </div>
                        <div className="w-32">
                            <label className="block text-sm font-medium mb-1">Quantity</label>
                            <Input
                                type="number"
                                min="1"
                                value={qty}
                                onChange={(e) => setQty(Number(e.target.value))}
                            />
                        </div>
                        <Button onClick={addItem} disabled={!selectedProduct}>
                            <Plus className="w-4 h-4 mr-2" /> Add
                        </Button>
                    </div>

                    <div className="border rounded">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                                <tr>
                                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase">Product</th>
                                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Quantity</th>
                                    <th className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase">Action</th>
                                </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                                {items.map((item) => (
                                    <tr key={item.productId}>
                                        <td className="px-6 py-4 text-sm font-medium text-gray-900">{item.productName}</td>
                                        <td className="px-6 py-4 text-sm text-right text-gray-500">{item.quantity}</td>
                                        <td className="px-6 py-4 text-right">
                                            <button onClick={() => removeItem(item.productId)} className="text-red-600 hover:text-red-900">
                                                <Trash2 className="w-4 h-4" />
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                                {items.length === 0 && (
                                    <tr>
                                        <td colSpan={3} className="px-6 py-4 text-center text-sm text-gray-500">No items added</td>
                                    </tr>
                                )}
                            </tbody>
                        </table>
                    </div>

                    <div className="flex justify-between mt-4">
                        <Button variant="secondary" onClick={() => setStep(1)}>
                            <ArrowLeft className="mr-2 w-4 h-4" /> Back
                        </Button>
                        <Button onClick={() => setStep(3)} disabled={items.length === 0}>
                            Next <ArrowRight className="ml-2 w-4 h-4" />
                        </Button>
                    </div>
                </div>
            )}

            {/* Step 3 */}
            {step === 3 && (
                <div className="space-y-4">
                    <div className="bg-gray-50 p-4 rounded space-y-2">
                        <p><strong>Source:</strong> {warehouses.find(w => w.id === sourceId)?.name}</p>
                        <p><strong>Destination:</strong> {warehouses.find(w => w.id === targetId)?.name}</p>
                        <p><strong>Items:</strong> {items.length}</p>
                    </div>

                    <div>
                        <label className="block text-sm font-medium mb-1">Notes</label>
                        <Input value={notes} onChange={(e) => setNotes(e.target.value)} placeholder="Optional notes..." />
                    </div>

                    <div className="flex justify-between mt-8">
                        <Button variant="secondary" onClick={() => setStep(2)}>
                            <ArrowLeft className="mr-2 w-4 h-4" /> Back
                        </Button>
                        <Button onClick={handleSubmit} disabled={loading}>
                            {loading ? 'Processing...' : (
                                <><Check className="mr-2 w-4 h-4" /> Submit Transfer</>
                            )}
                        </Button>
                    </div>
                </div>
            )}
        </div>
    );
}
