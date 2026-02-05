
import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Save, Barcode, Tag, Layers, Package, AlertCircle, Plus, Edit2, Trash2 } from 'lucide-react';
import { api } from '@/services/api';
import { Button, Input, Badge, useToast } from '@/components/ui';
import { ProductVariantDialog } from './ProductVariantDialog';

interface ProductDetailPageProps {
    mode: 'create' | 'edit';
}

interface ProductFormData {
    sku: string;
    name: string;
    type: number;
    unitOfMeasureId: string;
    categoryId: string;
    description: string;
    salesPrice: number;
    purchasePrice: number;
    minStockLevel: number;
    reorderLevel: number;
    isActive: boolean;
}

const PRODUCT_TYPES = [
    { value: 1, label: 'Inventory' },
    { value: 2, label: 'Service' },
    { value: 3, label: 'Non-Inventory' },
];

export function ProductDetailPage({ mode }: ProductDetailPageProps) {
    const { t } = useTranslation();
    const navigate = useNavigate();
    const { id } = useParams();
    const toast = useToast();

    // Helper state
    const [isLoading, setIsLoading] = useState(mode === 'edit');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [activeTab, setActiveTab] = useState('general');

    // Data state
    const [categories, setCategories] = useState<any[]>([]);
    const [uoms, setUoms] = useState<any[]>([]);
    const [product, setProduct] = useState<any>(null); // Full product object

    // Variants State
    const [variants, setVariants] = useState<any[]>([]);
    const [isVariantDialogOpen, setIsVariantDialogOpen] = useState(false);
    const [selectedVariantId, setSelectedVariantId] = useState<string | null>(null);

    // Form state
    const [formData, setFormData] = useState<ProductFormData>({
        sku: '',
        name: '',
        type: 1,
        unitOfMeasureId: '',
        categoryId: '',
        description: '',
        salesPrice: 0,
        purchasePrice: 0,
        minStockLevel: 0,
        reorderLevel: 0,
        isActive: true,
    });
    const [formErrors, setFormErrors] = useState<Partial<ProductFormData>>({});

    const loadDependencies = useCallback(async () => {
        try {
            const [catsRes, uomsRes] = await Promise.all([
                api.productCategories.getAll(),
                api.unitOfMeasures.getAll()
            ]);

            if (catsRes.success && catsRes.data) setCategories(catsRes.data);
            if (uomsRes.success && uomsRes.data) setUoms(uomsRes.data);
        } catch (error) {
            console.error("Failed to load dependencies", error);
            toast.error(t('common.error'), "Failed to load dependencies");
        }
    }, [t, toast]);

    const loadProduct = useCallback(async () => {
        if (mode === 'create' || !id) return;

        setIsLoading(true);
        try {
            const result = await api.products.getById(id);
            if (result.success && result.data) {
                const p = result.data;
                setProduct(p);
                setFormData({
                    sku: p.sku,
                    name: p.name,
                    type: p.type,
                    unitOfMeasureId: p.unitOfMeasureId || '',
                    categoryId: p.categoryId || '',
                    description: p.description || '',
                    salesPrice: p.salesPrice || 0,
                    purchasePrice: p.purchasePrice || 0,
                    minStockLevel: p.minStockLevel || 0,
                    reorderLevel: p.reorderLevel || 0,
                    isActive: p.isActive,
                });
            } else {
                toast.error(t('common.error'), result.error || "Product not found");
                navigate('/inventory/products');
            }
        } catch (error) {
            toast.error(t('common.error'), "Failed to load product");
            navigate('/inventory/products');
        } finally {
            setIsLoading(false);
        }
    }, [mode, id, navigate, t, toast]);

    const loadVariants = useCallback(async () => {
        if (!id) return;
        try {
            const res = await api.productVariants.getByProductId(id);
            if (res.success && res.data) {
                setVariants(res.data);
            }
        } catch (err) {
            console.error(err);
        }
    }, [id]);

    useEffect(() => {
        loadDependencies();
        loadProduct();
    }, [loadDependencies, loadProduct]);

    useEffect(() => {
        if (activeTab === 'variants' && id) {
            loadVariants();
        }
    }, [activeTab, id, loadVariants]);

    const validateForm = (): boolean => {
        const errors: Partial<ProductFormData> = {};
        if (!formData.sku.trim()) errors.sku = t('common.required');
        if (!formData.name.trim()) errors.name = t('common.required');
        if (!formData.unitOfMeasureId) errors.unitOfMeasureId = t('common.required');

        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async () => {
        if (!validateForm()) return;

        setIsSubmitting(true);
        try {
            const payload = {
                ...formData,
                categoryId: formData.categoryId || null,
            };

            if (mode === 'edit' && id) {
                const result = await api.products.update(id, payload);
                if (result.success) {
                    toast.success(t('inventory.productUpdated'));
                    // Reload to ensure state consistency
                    loadProduct();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } else {
                const result = await api.products.create(payload);
                if (result.success && result.data) {
                    toast.success(t('inventory.productCreated'));
                    // Navigate to edit mode for the new product
                    navigate(`/inventory/products/${result.data.id}`);
                } else {
                    toast.error(t('common.error'), result.error);
                }
            }
        } catch (err) {
            toast.error(t('common.error'), (err as Error).message);
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDeleteVariant = async (variantId: string) => {
        if (!window.confirm(t('common.confirmDelete'))) return;
        try {
            const res = await api.productVariants.delete(variantId);
            if (res.success) {
                toast.success(t('common.deleted'));
                loadVariants();
            } else {
                toast.error(t('common.error'), res.error);
            }
        } catch (err: any) {
            toast.error(t('common.error'), err.message);
        }
    };

    // Format attributes for display
    const formatAttributes = (attrsJson: string) => {
        try {
            const parsed = JSON.parse(attrsJson || '{}');
            return Object.entries(parsed).map(([k, v]) => `${k}: ${v}`).join(', ');
        } catch {
            return '-';
        }
    };

    if (isLoading) {
        return (
            <div className="flex items-center justify-center min-h-[400px]">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
            </div>
        );
    }

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div className="flex items-center gap-4">
                    <Button variant="ghost" onClick={() => navigate('/inventory/products')}>
                        <ArrowLeft className="w-4 h-4 mr-2" />
                        {t('common.back')}
                    </Button>
                    <div>
                        <h1 className="text-2xl font-bold">
                            {mode === 'create' ? t('inventory.createProduct') : product?.name || t('inventory.editProduct')}
                        </h1>
                        {mode === 'edit' && product && (
                            <div className="flex items-center gap-2 mt-1">
                                <span className="font-mono text-xs bg-[hsl(var(--secondary))] text-[hsl(var(--secondary-foreground))] px-2 py-0.5 rounded-full">{product.sku}</span>
                                {product.isActive ? (
                                    <Badge variant="success">{t('common.active')}</Badge>
                                ) : (
                                    <Badge variant="default">{t('common.inactive')}</Badge>
                                )}
                            </div>
                        )}
                    </div>
                </div>
                <div className="flex items-center gap-2">
                    <Button onClick={handleSubmit} isLoading={isSubmitting}>
                        <Save className="w-4 h-4 mr-2" />
                        {t('common.save')}
                    </Button>
                </div>
            </div>

            {/* Tabs Navigation (only relevant if Edit, or just show General for Create) */}
            <div className="border-b border-[hsl(var(--border))]">
                <nav className="flex space-x-8 overflow-x-auto" aria-label="Tabs">
                    <button
                        onClick={() => setActiveTab('general')}
                        className={`
                            whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm flex items-center gap-2
                            ${activeTab === 'general'
                                ? 'border-[hsl(var(--primary))] text-[hsl(var(--primary))]'
                                : 'border-transparent text-[hsl(var(--muted-foreground))] hover:text-[hsl(var(--foreground))] hover:border-[hsl(var(--border))]'}
                        `}
                    >
                        <Package className="w-4 h-4" />
                        {t('nav.general')}
                    </button>

                    {mode === 'edit' && (
                        <>
                            <button
                                onClick={() => setActiveTab('variants')}
                                className={`
                                    whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm flex items-center gap-2
                                    ${activeTab === 'variants'
                                        ? 'border-[hsl(var(--primary))] text-[hsl(var(--primary))]'
                                        : 'border-transparent text-[hsl(var(--muted-foreground))] hover:text-[hsl(var(--foreground))] hover:border-[hsl(var(--border))]'}
                                `}
                            >
                                <Layers className="w-4 h-4" />
                                {t('inventory.variants')}
                            </button>
                            <button
                                onClick={() => setActiveTab('barcodes')}
                                className={`
                                    whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm flex items-center gap-2
                                    ${activeTab === 'barcodes'
                                        ? 'border-[hsl(var(--primary))] text-[hsl(var(--primary))]'
                                        : 'border-transparent text-[hsl(var(--muted-foreground))] hover:text-[hsl(var(--foreground))] hover:border-[hsl(var(--border))]'}
                                `}
                            >
                                <Barcode className="w-4 h-4" />
                                {t('inventory.barcodes')}
                            </button>
                            <button
                                onClick={() => setActiveTab('prices')}
                                className={`
                                    whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm flex items-center gap-2
                                    ${activeTab === 'prices'
                                        ? 'border-[hsl(var(--primary))] text-[hsl(var(--primary))]'
                                        : 'border-transparent text-[hsl(var(--muted-foreground))] hover:text-[hsl(var(--foreground))] hover:border-[hsl(var(--border))]'}
                                `}
                            >
                                <Tag className="w-4 h-4" />
                                {t('inventory.prices')}
                            </button>
                            <button
                                onClick={() => setActiveTab('stock')}
                                className={`
                                    whitespace-nowrap py-4 px-1 border-b-2 font-medium text-sm flex items-center gap-2
                                    ${activeTab === 'stock'
                                        ? 'border-[hsl(var(--primary))] text-[hsl(var(--primary))]'
                                        : 'border-transparent text-[hsl(var(--muted-foreground))] hover:text-[hsl(var(--foreground))] hover:border-[hsl(var(--border))]'}
                                `}
                            >
                                <Layers className="w-4 h-4" />
                                {t('inventory.stock')}
                            </button>
                        </>
                    )}
                </nav>
            </div>

            <div className="mt-6">
                {activeTab === 'general' && (
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 items-start">
                        {/* Left Column: Core Info */}
                        <div className="space-y-6 bg-[hsl(var(--card))] p-6 rounded-lg border border-[hsl(var(--border))] shadow-sm">
                            <h3 className="text-lg font-medium mb-4">Basic Information</h3>

                            <Input
                                label={t('inventory.sku')}
                                value={formData.sku}
                                onChange={(e) => setFormData({ ...formData, sku: e.target.value })}
                                error={formErrors.sku}
                                required
                            />

                            <Input
                                label={t('common.name')}
                                value={formData.name}
                                onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                                error={formErrors.name}
                                required
                            />

                            <div className="space-y-2">
                                <label className="text-sm font-medium">Type</label>
                                <select
                                    className="w-full h-10 px-3 py-2 rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-[hsl(var(--primary))]"
                                    value={formData.type}
                                    onChange={(e) => setFormData({ ...formData, type: parseInt(e.target.value) })}
                                >
                                    {PRODUCT_TYPES.map(t => (
                                        <option key={t.value} value={t.value}>{t.label}</option>
                                    ))}
                                </select>
                            </div>

                            <div className="space-y-2">
                                <label className="text-sm font-medium">{t('inventory.categoryName')} (Category)</label>
                                <select
                                    className="w-full h-10 px-3 py-2 rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-[hsl(var(--primary))]"
                                    value={formData.categoryId}
                                    onChange={(e) => setFormData({ ...formData, categoryId: e.target.value })}
                                >
                                    <option value="">{t('common.select')}</option>
                                    {categories.map(c => (
                                        <option key={c.id} value={c.id}>{c.name}</option>
                                    ))}
                                </select>
                            </div>

                            <div className="space-y-2">
                                <label className="text-sm font-medium">Unit of Measure</label>
                                <select
                                    className={`w-full h-10 px-3 py-2 rounded-md border bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-[hsl(var(--primary))] ${formErrors.unitOfMeasureId ? 'border-red-500' : 'border-[hsl(var(--input))]'}`}
                                    value={formData.unitOfMeasureId}
                                    onChange={(e) => setFormData({ ...formData, unitOfMeasureId: e.target.value })}
                                >
                                    <option value="">{t('common.select')}</option>
                                    {uoms.map(u => (
                                        <option key={u.id} value={u.id}>{u.name} ({u.code})</option>
                                    ))}
                                </select>
                                {formErrors.unitOfMeasureId && <p className="text-xs text-red-500">{formErrors.unitOfMeasureId}</p>}
                            </div>

                            <div className="flex items-center h-full pt-4">
                                <label className="flex items-center gap-2 cursor-pointer">
                                    <input
                                        type="checkbox"
                                        checked={formData.isActive}
                                        onChange={(e) => setFormData({ ...formData, isActive: e.target.checked })}
                                        className="rounded border-[hsl(var(--border))]"
                                    />
                                    <span>{t('common.isActive')}</span>
                                </label>
                            </div>
                        </div>

                        {/* Right Column: Pricing & Inventory Settings */}
                        <div className="space-y-6">
                            <div className="bg-[hsl(var(--card))] p-6 rounded-lg border border-[hsl(var(--border))] shadow-sm space-y-4">
                                <h3 className="text-lg font-medium mb-2">Pricing (Base)</h3>
                                <div className="grid grid-cols-2 gap-4">
                                    <Input
                                        label={t('inventory.salesPrice')}
                                        type="number"
                                        min="0"
                                        step="0.01"
                                        value={formData.salesPrice}
                                        onChange={(e) => setFormData({ ...formData, salesPrice: parseFloat(e.target.value) || 0 })}
                                    />
                                    <Input
                                        label={t('inventory.purchasePrice')}
                                        type="number"
                                        min="0"
                                        step="0.01"
                                        value={formData.purchasePrice}
                                        onChange={(e) => setFormData({ ...formData, purchasePrice: parseFloat(e.target.value) || 0 })}
                                    />
                                </div>
                            </div>

                            <div className="bg-[hsl(var(--card))] p-6 rounded-lg border border-[hsl(var(--border))] shadow-sm space-y-4">
                                <h3 className="text-lg font-medium mb-2">Inventory Settings</h3>
                                <div className="grid grid-cols-2 gap-4">
                                    <Input
                                        label={t('inventory.minStock')}
                                        type="number"
                                        min="0"
                                        value={formData.minStockLevel}
                                        onChange={(e) => setFormData({ ...formData, minStockLevel: parseInt(e.target.value) || 0 })}
                                    />
                                    <Input
                                        label={t('inventory.reorderLevel')}
                                        type="number"
                                        min="0"
                                        value={formData.reorderLevel}
                                        onChange={(e) => setFormData({ ...formData, reorderLevel: parseInt(e.target.value) || 0 })}
                                    />
                                </div>
                            </div>
                        </div>
                    </div>
                )}

                {activeTab === 'variants' && (
                    <div className="bg-[hsl(var(--card))] rounded-lg border border-[hsl(var(--border))] shadow-sm">
                        <div className="p-4 border-b border-[hsl(var(--border))] flex justify-between items-center">
                            <h2 className="text-lg font-semibold">{t('inventory.variants')}</h2>
                            <Button size="sm" onClick={() => { setSelectedVariantId(null); setIsVariantDialogOpen(true); }}>
                                <Plus className="w-4 h-4 mr-2" />
                                {t('inventory.addVariant')}
                            </Button>
                        </div>

                        <div className="overflow-x-auto">
                            <table className="w-full text-sm text-left">
                                <thead className="bg-[hsl(var(--muted))] text-[hsl(var(--muted-foreground))] uppercase font-medium">
                                    <tr>
                                        <th className="px-4 py-3">{t('inventory.code')}</th>
                                        <th className="px-4 py-3">{t('common.name')}</th>
                                        <th className="px-4 py-3">{t('inventory.attributes')}</th>
                                        <th className="px-4 py-3 text-right">{t('common.actions')}</th>
                                    </tr>
                                </thead>
                                <tbody className="divide-y divide-[hsl(var(--border))]">
                                    {variants.length === 0 ? (
                                        <tr>
                                            <td colSpan={4} className="px-4 py-8 text-center text-muted-foreground">
                                                {t('inventory.noVariants')}
                                            </td>
                                        </tr>
                                    ) : (
                                        variants.map((v) => (
                                            <tr key={v.id} className="hover:bg-[hsl(var(--muted))/50]">
                                                <td className="px-4 py-3 font-medium">{v.code}</td>
                                                <td className="px-4 py-3">{v.name}</td>
                                                <td className="px-4 py-3 text-muted-foreground">{formatAttributes(v.attributes)}</td>
                                                <td className="px-4 py-3 text-right">
                                                    <div className="flex justify-end gap-2">
                                                        <Button variant="ghost" size="icon" onClick={() => { setSelectedVariantId(v.id); setIsVariantDialogOpen(true); }}>
                                                            <Edit2 className="w-4 h-4" />
                                                        </Button>
                                                        <Button variant="ghost" size="icon" className="text-destructive hover:text-destructive" onClick={() => handleDeleteVariant(v.id)}>
                                                            <Trash2 className="w-4 h-4" />
                                                        </Button>
                                                    </div>
                                                </td>
                                            </tr>
                                        ))
                                    )}
                                </tbody>
                            </table>
                        </div>

                        {id && (
                            <ProductVariantDialog
                                open={isVariantDialogOpen}
                                onClose={(refresh) => {
                                    setIsVariantDialogOpen(false);
                                    if (refresh) loadVariants();
                                }}
                                productId={id}
                                variantId={selectedVariantId}
                            />
                        )}
                    </div>
                )}

                {(activeTab === 'barcodes' || activeTab === 'prices' || activeTab === 'stock') && (
                    <div className="p-8 border-2 border-dashed rounded-lg text-center text-[hsl(var(--muted-foreground))] flex flex-col items-center justify-center">
                        <AlertCircle className="w-8 h-8 mb-2 opacity-50" />
                        <p>This tab is currently under construction.</p>
                        <p className="text-sm">Functionality for {activeTab} will be available in the next update.</p>
                    </div>
                )}
            </div>
        </div>
    );
}
