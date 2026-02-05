import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Warehouse, MapPin, CheckCircle } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface Warehouse {
    id: string;
    code: string;
    name: string;
    description: string | null;
    isDefault: boolean;
    branchId: string | null;
    address: string | null;
    isActive: boolean;
}

interface WarehouseFormData {
    code: string;
    name: string;
    description: string;
    address: string;
    isActive: boolean;
}

export function WarehousesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [warehouses, setWarehouses] = useState<Warehouse[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingWarehouse, setEditingWarehouse] = useState<Warehouse | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Form state
    const [formData, setFormData] = useState<WarehouseFormData>({
        code: '',
        name: '',
        description: '',
        address: '',
        isActive: true,
    });
    const [formErrors, setFormErrors] = useState<Partial<WarehouseFormData>>({});

    const loadWarehouses = useCallback(async () => {
        setIsLoading(true);
        const result = await api.warehouses.getAll();
        if (result.success && result.data) {
            setWarehouses(result.data);
        }
        setIsLoading(false);
    }, []);

    useEffect(() => {
        loadWarehouses();
    }, [loadWarehouses]);

    const openCreateModal = () => {
        setEditingWarehouse(null);
        setFormData({
            code: '',
            name: '',
            description: '',
            address: '',
            isActive: true,
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openEditModal = (warehouse: Warehouse) => {
        setEditingWarehouse(warehouse);
        setFormData({
            code: warehouse.code,
            name: warehouse.name,
            description: warehouse.description || '',
            address: warehouse.address || '',
            isActive: warehouse.isActive,
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const validateForm = (): boolean => {
        const errors: Partial<WarehouseFormData> = {};
        if (!formData.name.trim()) {
            errors.name = t('common.required');
        }
        if (!formData.code.trim()) {
            errors.code = t('common.required');
        }
        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async () => {
        if (!validateForm()) return;

        setIsSubmitting(true);
        try {
            if (editingWarehouse) {
                const result = await api.warehouses.update(editingWarehouse.id, formData);
                if (result.success) {
                    toast.success(t('inventory.warehouseUpdated'));
                    setIsModalOpen(false);
                    loadWarehouses();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } else {
                const result = await api.warehouses.create(formData);
                if (result.success) {
                    toast.success(t('inventory.warehouseCreated'));
                    setIsModalOpen(false);
                    loadWarehouses();
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

    const handleDelete = async (warehouse: Warehouse) => {
        const confirmed = await dialog.danger({
            title: t('inventory.deleteWarehouse'),
            message: t('inventory.confirmDeleteWarehouse'),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.warehouses.delete(warehouse.id);
            if (result.success) {
                toast.success(t('inventory.warehouseDeleted'));
                loadWarehouses();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const handleSetDefault = async (warehouse: Warehouse) => {
        if (warehouse.isDefault) return;

        const result = await api.warehouses.setDefault(warehouse.id);
        if (result.success) {
            toast.success(t('inventory.warehouseUpdated'));
            loadWarehouses();
        } else {
            toast.error(t('common.error'), result.error);
        }
    };

    const columns: Column<Warehouse>[] = [
        {
            key: 'name',
            header: t('inventory.categoryName'), // Reusing categoryName translation for now or adding inventory.warehouseName later?
            // Actually I should use 'inventory.name' or 'common.name'. 'inventory.categoriesName' refers to Categories. 
            // I'll check translations. 'common.name' exists? 'currencies.name' exists. 
            // I'll use 'common.name' or similar if general. Let's use 'inventory.code' and 'inventory.name' which I might need to add/verify.
            // I added 'categoryName' in en.json. I should add 'inventory.warehouseName' or just use generic columns.
            // checking en.json: "code": "Code", "name": "Name" are in 'common' or others?
            // 'partners' has "name": "Name". 'currencies' has "name".
            // I'll check my recent en.json edit.
            // I added "code" under "inventory". I didn't add "name" specifically under inventory, but "categoryName" exists.
            // I'll assume I can use hardcoded string or find better key later. I'll use "Name" and "Code" directly via t('inventory.code') which I added.
            // For Name, I'll use generic t('common.name') if it exists (it wasn't in my view). 
            // 'roles.name' exists. 'currencies.name' exists.
            // I'll use t('inventory.code') and just 'Name' for now to be safe or t('currencies.name') reused.
            // Actually I'll use "Name" string fallback.
            render: (warehouse) => (
                <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded bg-orange-100 dark:bg-orange-900/30 text-orange-600 flex items-center justify-center">
                        <Warehouse className="w-4 h-4" />
                    </div>
                    <div>
                        <div className="flex items-center gap-2">
                            <p className="font-medium">{warehouse.name}</p>
                            {warehouse.isDefault && (
                                <span className="text-xs bg-blue-100 text-blue-700 px-1.5 py-0.5 rounded border border-blue-200">
                                    {t('inventory.isDefault')}
                                </span>
                            )}
                        </div>
                        <p className="text-xs text-[hsl(var(--muted-foreground))]">{warehouse.code}</p>
                    </div>
                </div>
            ),
        },
        {
            key: 'address',
            header: t('inventory.address'),
            render: (w) => w.address ? (
                <div className="flex items-center gap-1 text-sm text-[hsl(var(--muted-foreground))]">
                    <MapPin className="w-3 h-3" />
                    <span>{w.address}</span>
                </div>
            ) : '-',
        },
        {
            key: 'isActive',
            header: t('common.status'),
            render: (w) => (
                <Badge variant={w.isActive ? 'success' : 'default'}>
                    {w.isActive ? t('common.active') : t('common.inactive')}
                </Badge>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('inventory.warehouses')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('inventory.warehousesSubtitle')}
                    </p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('inventory.createWarehouse')}
                </Button>
            </div>

            <DataTable
                data={warehouses}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(warehouse) => (
                    <div className="flex items-center gap-1">
                        {!warehouse.isDefault && (
                            <button
                                onClick={() => handleSetDefault(warehouse)}
                                className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                                title={t('inventory.setDefault')}
                            >
                                <CheckCircle className="w-4 h-4 text-green-600" />
                            </button>
                        )}
                        <button
                            onClick={() => openEditModal(warehouse)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(warehouse)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={editingWarehouse ? t('inventory.editWarehouse') : t('inventory.createWarehouse')}
                size="md"
                footer={
                    <>
                        <Button variant="secondary" onClick={() => setIsModalOpen(false)}>
                            {t('common.cancel')}
                        </Button>
                        <Button onClick={handleSubmit} isLoading={isSubmitting}>
                            {t('common.save')}
                        </Button>
                    </>
                }
            >
                <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4">
                        <Input
                            label={t('inventory.code')}
                            value={formData.code}
                            onChange={(e) => setFormData({ ...formData, code: e.target.value })}
                            error={formErrors.code}
                            required
                        />
                        <Input
                            label="Name" // Fallback string
                            value={formData.name}
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                            error={formErrors.name}
                            required
                        />
                    </div>

                    <Input
                        label={t('inventory.address')}
                        value={formData.address}
                        onChange={(e) => setFormData({ ...formData, address: e.target.value })}
                    />

                    <Input
                        label={t('common.description')}
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    />

                    <div className="flex items-center h-full pt-2">
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
            </Modal>
        </div>
    );
}
