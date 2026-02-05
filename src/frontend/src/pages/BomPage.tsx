import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Eye, Trash2, Package, Layers } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Select, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface Bom {
    id: string;
    code: string;
    name: string;
    productId: string;
    productName?: string;
    quantity: number;
    type: number;
    typeName: string;
    isDefault: boolean;
    componentCount: number;
    createdAt: string;
}

interface BomComponent {
    id: string;
    productId: string;
    productName?: string;
    quantity: number;
    lineNumber: number;
}

interface Product {
    id: string;
    name: string;
    sku: string;
}

const BOM_TYPES = [
    { value: 0, label: 'Standard' },
    { value: 1, label: 'Phantom' },
    { value: 2, label: 'Engineering' },
];

export function BomPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [boms, setBoms] = useState<Bom[]>([]);
    const [products, setProducts] = useState<Product[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [page, setPage] = useState(1);
    const [total, setTotal] = useState(0);
    const pageSize = 20;

    // Create Modal
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [formData, setFormData] = useState({
        code: '',
        name: '',
        productId: '',
        quantity: 1,
        type: 0,
        isDefault: true,
    });

    // Detail Modal
    const [isDetailModalOpen, setIsDetailModalOpen] = useState(false);
    const [selectedBom, setSelectedBom] = useState<Bom | null>(null);
    const [components, setComponents] = useState<BomComponent[]>([]);

    // Add Component
    const [isAddComponentOpen, setIsAddComponentOpen] = useState(false);
    const [componentData, setComponentData] = useState({ productId: '', quantity: 1 });

    const fetchBoms = useCallback(async () => {
        setIsLoading(true);
        const result = await api.bom.getAll(page, pageSize);
        if (result.success && result.data) {
            setBoms(result.data.data || []);
            setTotal(result.data.totalCount || 0);
        }
        setIsLoading(false);
    }, [page]);

    const fetchProducts = async () => {
        const result = await api.products.getAll();
        if (result.success && result.data) {
            setProducts(result.data || []);
        }
    };

    useEffect(() => {
        fetchBoms();
        fetchProducts();
    }, [fetchBoms]);

    const handleCreate = async () => {
        const result = await api.bom.create(formData);
        if (result.success) {
            toast.success(t('manufacturing.bom.created'));
            setIsModalOpen(false);
            setFormData({ code: '', name: '', productId: '', quantity: 1, type: 0, isDefault: true });
            fetchBoms();
        } else {
            toast.error(t('common.error'), result.error);
        }
    };

    const handleDelete = async (bom: Bom) => {
        const confirmed = await dialog.danger({
            title: t('common.confirmDelete'),
            message: t('manufacturing.bom.deleteConfirm', { name: bom.name }),
            confirmText: t('common.delete'),
        });
        if (confirmed) {
            const result = await api.bom.delete(bom.id);
            if (result.success) {
                toast.success(t('manufacturing.bom.deleted'));
                fetchBoms();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const handleViewDetails = async (bom: Bom) => {
        const result = await api.bom.getById(bom.id);
        if (result.success && result.data) {
            setSelectedBom(result.data);
            setComponents(result.data.components || []);
            setIsDetailModalOpen(true);
        }
    };

    const handleAddComponent = async () => {
        if (!selectedBom) return;
        const result = await api.bom.addComponent(selectedBom.id, componentData);
        if (result.success) {
            toast.success(t('manufacturing.bom.componentAdded'));
            setIsAddComponentOpen(false);
            setComponentData({ productId: '', quantity: 1 });
            handleViewDetails(selectedBom);
        } else {
            toast.error(t('common.error'), result.error);
        }
    };

    const handleRemoveComponent = async (componentId: string) => {
        const result = await api.bom.removeComponent(componentId);
        if (result.success) {
            toast.success(t('manufacturing.bom.componentRemoved'));
            if (selectedBom) handleViewDetails(selectedBom);
        }
    };

    const columns: Column<Bom>[] = [
        { key: 'code', header: t('manufacturing.bom.code') },
        { key: 'name', header: t('manufacturing.bom.name') },
        {
            key: 'productId',
            header: t('manufacturing.bom.product'),
            render: (bom) => products.find(p => p.id === bom.productId)?.name || '-',
        },
        { key: 'quantity', header: t('manufacturing.bom.quantity') },
        {
            key: 'type',
            header: t('manufacturing.bom.type'),
            render: (bom) => <Badge variant="info">{bom.typeName}</Badge>,
        },
        { key: 'componentCount', header: t('manufacturing.bom.components') },
        {
            key: 'isDefault',
            header: t('manufacturing.bom.default'),
            render: (bom) => bom.isDefault ? <Badge variant="success">✓</Badge> : null,
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex justify-between items-center">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Layers className="w-7 h-7" />
                        {t('manufacturing.bom.title')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('manufacturing.bom.subtitle')}</p>
                </div>
                <Button onClick={() => setIsModalOpen(true)}>
                    <Plus className="w-4 h-4" />
                    {t('manufacturing.bom.create')}
                </Button>
            </div>

            <DataTable
                data={boms}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                pagination={{ page, pageSize, total, onPageChange: setPage }}
                actions={(bom) => (
                    <div className="flex gap-2">
                        <button onClick={() => handleViewDetails(bom)} className="p-2 rounded-lg hover:bg-blue-100 dark:hover:bg-blue-900/30 text-blue-600">
                            <Eye className="w-4 h-4" />
                        </button>
                        <button onClick={() => handleDelete(bom)} className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600">
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            {/* Create Modal */}
            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={t('manufacturing.bom.create')}
                size="md"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleCreate}>{t('common.save')}</Button>
                    </>
                }
            >
                <div className="space-y-4">
                    <Input label={t('manufacturing.bom.code')} value={formData.code} onChange={(e) => setFormData({ ...formData, code: e.target.value })} required />
                    <Input label={t('manufacturing.bom.name')} value={formData.name} onChange={(e) => setFormData({ ...formData, name: e.target.value })} required />
                    <Select
                        label={t('manufacturing.bom.product')}
                        value={formData.productId}
                        onChange={(e) => setFormData({ ...formData, productId: e.target.value })}
                        options={products.map(p => ({ value: p.id, label: p.name }))}
                    />
                    <Input label={t('manufacturing.bom.quantity')} type="number" value={formData.quantity} onChange={(e) => setFormData({ ...formData, quantity: Number(e.target.value) })} required />
                    <Select
                        label={t('manufacturing.bom.type')}
                        value={String(formData.type)}
                        onChange={(e) => setFormData({ ...formData, type: Number(e.target.value) })}
                        options={BOM_TYPES.map(t => ({ value: String(t.value), label: t.label }))}
                    />
                </div>
            </Modal>

            {/* Detail Modal */}
            <Modal isOpen={isDetailModalOpen} onClose={() => setIsDetailModalOpen(false)} title={selectedBom?.name || ''} size="lg">
                <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4 text-sm">
                        <div><span className="font-medium">{t('manufacturing.bom.code')}:</span> {selectedBom?.code}</div>
                        <div><span className="font-medium">{t('manufacturing.bom.type')}:</span> {selectedBom?.typeName}</div>
                        <div><span className="font-medium">{t('manufacturing.bom.quantity')}:</span> {selectedBom?.quantity}</div>
                        <div><span className="font-medium">{t('manufacturing.bom.default')}:</span> {selectedBom?.isDefault ? 'Yes' : 'No'}</div>
                    </div>

                    <div className="border-t pt-4">
                        <div className="flex justify-between items-center mb-2">
                            <h3 className="font-semibold flex items-center gap-2">
                                <Package className="w-4 h-4" />
                                {t('manufacturing.bom.components')}
                            </h3>
                            <Button size="sm" onClick={() => setIsAddComponentOpen(true)}>
                                <Plus className="w-3 h-3" />
                                {t('manufacturing.bom.addComponent')}
                            </Button>
                        </div>
                        <table className="w-full text-sm">
                            <thead className="bg-[hsl(var(--accent))]">
                                <tr>
                                    <th className="px-3 py-2 text-left">#</th>
                                    <th className="px-3 py-2 text-left">{t('manufacturing.bom.product')}</th>
                                    <th className="px-3 py-2 text-right">{t('manufacturing.bom.quantity')}</th>
                                    <th className="px-3 py-2"></th>
                                </tr>
                            </thead>
                            <tbody>
                                {components.map((c, idx) => (
                                    <tr key={c.id} className="border-b">
                                        <td className="px-3 py-2">{idx + 1}</td>
                                        <td className="px-3 py-2">{products.find(p => p.id === c.productId)?.name || c.productId}</td>
                                        <td className="px-3 py-2 text-right">{c.quantity}</td>
                                        <td className="px-3 py-2">
                                            <button onClick={() => handleRemoveComponent(c.id)} className="text-red-600">
                                                <Trash2 className="w-4 h-4" />
                                            </button>
                                        </td>
                                    </tr>
                                ))}
                                {components.length === 0 && (
                                    <tr><td colSpan={4} className="text-center py-4 text-[hsl(var(--muted-foreground))]">{t('common.noData')}</td></tr>
                                )}
                            </tbody>
                        </table>
                    </div>
                </div>
            </Modal>

            {/* Add Component Modal */}
            <Modal
                isOpen={isAddComponentOpen}
                onClose={() => setIsAddComponentOpen(false)}
                title={t('manufacturing.bom.addComponent')}
                size="md"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsAddComponentOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleAddComponent}>{t('common.save')}</Button>
                    </>
                }
            >
                <div className="space-y-4">
                    <Select
                        label={t('manufacturing.bom.product')}
                        value={componentData.productId}
                        onChange={(e) => setComponentData({ ...componentData, productId: e.target.value })}
                        options={products.map(p => ({ value: p.id, label: p.name }))}
                    />
                    <Input
                        label={t('manufacturing.bom.quantity')}
                        type="number"
                        value={componentData.quantity}
                        onChange={(e) => setComponentData({ ...componentData, quantity: Number(e.target.value) })}
                    />
                </div>
            </Modal>
        </div>
    );
}
