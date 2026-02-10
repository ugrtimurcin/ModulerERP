import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Scale, Ruler, Box, Layers, Archive } from 'lucide-react';
import { api } from '@/services/api';
import { DataTable, Button, Modal, Input, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';

interface UnitOfMeasure {
    id: string;
    code: string;
    name: string;
    type: number;
    baseUnitId: string | null;
    baseUnitCode: string | null;
    conversionFactor: number;
    isActive: boolean;
}

interface UomFormData {
    code: string;
    name: string;
    type: number;
    baseUnitId: string;
    conversionFactor: number;
    isActive: boolean;
}



export function UnitOfMeasuresPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const UOM_TYPES = [
        { value: 1, label: t('inventory.uomTypes.1'), icon: Layers },
        { value: 2, label: t('inventory.uomTypes.2'), icon: Scale },
        { value: 3, label: t('inventory.uomTypes.3'), icon: Archive },
        { value: 4, label: t('inventory.uomTypes.4'), icon: Ruler },
        { value: 5, label: t('inventory.uomTypes.5'), icon: Box },
    ];

    const [uoms, setUoms] = useState<UnitOfMeasure[]>([]);
    const [isLoading, setIsLoading] = useState(true);

    // For Base Unit Dropdown
    const [baseUnits, setBaseUnits] = useState<UnitOfMeasure[]>([]);

    // Modal state
    const [isModalOpen, setIsModalOpen] = useState(false);
    const [editingUom, setEditingUom] = useState<UnitOfMeasure | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);

    // Form state
    const [formData, setFormData] = useState<UomFormData>({
        code: '',
        name: '',
        type: 1,
        baseUnitId: '',
        conversionFactor: 1,
        isActive: true,
    });
    const [formErrors, setFormErrors] = useState<Record<string, string>>({});

    const loadUoms = useCallback(async () => {
        setIsLoading(true);
        const result = await api.unitOfMeasures.getAll();
        if (result.success && result.data) {
            setUoms(result.data);
        }
        setIsLoading(false);
    }, []);

    useEffect(() => {
        loadUoms();
    }, [loadUoms]);

    // When type changes or modal opens, filter potential base units
    useEffect(() => {
        if (isModalOpen) {
            // Filter UOMs that are of the same type AND are Base Units (no baseUnitId)
            // Or should I allow chaining? Usually UOMs are relative to a SINGLE Base Unit per type.
            // My backend implementation enforces "Base Unit" as one with no parent. 
            // So logic: Base Unit has BaseUnitId = null. Derived has BaseUnitId = someId.
            // The dropdown should show ONLY Base Units of the selected Type.
            const possibleBaseUnits = uoms.filter(u => u.type === formData.type && u.baseUnitId === null && u.isActive);
            setBaseUnits(possibleBaseUnits);
        }
    }, [isModalOpen, formData.type, uoms]);

    const openCreateModal = () => {
        setEditingUom(null);
        setFormData({
            code: '',
            name: '',
            type: 1,
            baseUnitId: '',
            conversionFactor: 1,
            isActive: true,
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const openEditModal = (uom: UnitOfMeasure) => {
        setEditingUom(uom);
        setFormData({
            code: uom.code,
            name: uom.name,
            type: uom.type,
            baseUnitId: uom.baseUnitId || '',
            conversionFactor: uom.conversionFactor,
            isActive: uom.isActive,
        });
        setFormErrors({});
        setIsModalOpen(true);
    };

    const validateForm = (): boolean => {
        const errors: Record<string, string> = {};
        if (!formData.code.trim()) {
            errors.code = t('common.required');
        }
        if (!formData.name.trim()) {
            errors.name = t('common.required');
        }
        if (formData.baseUnitId && formData.conversionFactor <= 0) {
            errors.conversionFactor = t('common.greaterThanZero');
        }

        setFormErrors(errors);
        return Object.keys(errors).length === 0;
    };

    const handleSubmit = async () => {
        if (!validateForm()) return;

        setIsSubmitting(true);
        try {
            const payload = {
                ...formData,
                baseUnitId: formData.baseUnitId || null,
                // conversionFactor is forced to 1 by backend if baseUnitId is null, but we send what form has or 1
                conversionFactor: formData.baseUnitId ? formData.conversionFactor : 1
            };

            if (editingUom) {
                const result = await api.unitOfMeasures.update(editingUom.id, payload);
                if (result.success) {
                    toast.success(t('inventory.uomUpdated'));
                    setIsModalOpen(false);
                    loadUoms();
                } else {
                    toast.error(t('common.error'), result.error);
                }
            } else {
                const result = await api.unitOfMeasures.create(payload);
                if (result.success) {
                    toast.success(t('inventory.uomCreated'));
                    setIsModalOpen(false);
                    loadUoms();
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

    const handleDelete = async (uom: UnitOfMeasure) => {
        const confirmed = await dialog.danger({
            title: t('inventory.deleteUom'),
            message: t('inventory.confirmDeleteUom'), // Add to translations!
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            const result = await api.unitOfMeasures.delete(uom.id);
            if (result.success) {
                toast.success(t('inventory.uomDeleted'));
                loadUoms();
            } else {
                toast.error(t('common.error'), result.error);
            }
        }
    };

    const columns: Column<UnitOfMeasure>[] = [
        {
            key: 'code',
            header: t('inventory.code'),
            render: (uom) => (
                <div className="flex items-center gap-3">
                    <div className="w-8 h-8 rounded bg-teal-100 dark:bg-teal-900/30 text-teal-600 flex items-center justify-center">
                        <Ruler className="w-4 h-4" />
                    </div>
                    <div>
                        <p className="font-medium">{uom.code}</p>
                        <p className="text-xs text-[hsl(var(--muted-foreground))]">{uom.name}</p>
                    </div>
                </div>
            ),
        },
        {
            key: 'type',
            header: t('inventory.type'),
            render: (uom) => {
                const typeInfo = UOM_TYPES.find(t => t.value === uom.type);
                return typeInfo ? typeInfo.label : uom.type;
            }
        },
        {
            key: 'conversionFactor',
            header: t('inventory.factor'),
            render: (uom) => (
                <div className="flex flex-col text-sm">
                    {uom.baseUnitCode ? (
                        <span>1 {uom.code} = {uom.conversionFactor} {uom.baseUnitCode}</span>
                    ) : (
                        <span className="text-[hsl(var(--muted-foreground))] italic">{t('inventory.baseUnit')}</span>
                    )}
                </div>
            )
        },
        {
            key: 'isActive',
            header: t('common.status'),
            render: (uom) => (
                <Badge variant={uom.isActive ? 'success' : 'default'}>
                    {uom.isActive ? t('common.active') : t('common.inactive')}
                </Badge>
            ),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('inventory.uom')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('inventory.uomSubtitle')}
                    </p>
                </div>
                <Button onClick={openCreateModal}>
                    <Plus className="w-4 h-4" />
                    {t('inventory.createUom')}
                </Button>
            </div>

            <DataTable
                data={uoms}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(uom) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => openEditModal(uom)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(uom)}
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
                title={editingUom ? t('inventory.editUom') : t('inventory.createUom')}
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
                            label={t('common.name')}
                            value={formData.name}
                            onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                            error={formErrors.name}
                            required
                        />
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium">{t('inventory.type')}</label>
                        <select
                            className="w-full h-10 px-3 py-2 rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-[hsl(var(--primary))]"
                            value={formData.type}
                            onChange={(e) => setFormData({ ...formData, type: parseInt(e.target.value), baseUnitId: '' })}
                            disabled={!!editingUom} // Type shouldn't change easily as checking dependency is hard
                        >
                            {UOM_TYPES.map(type => (
                                <option key={type.value} value={type.value}>{type.label}</option>
                            ))}
                        </select>
                    </div>

                    <div className="space-y-2">
                        <label className="text-sm font-medium">{t('inventory.baseUnit')}</label>
                        <select
                            className="w-full h-10 px-3 py-2 rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-[hsl(var(--primary))]"
                            value={formData.baseUnitId}
                            onChange={(e) => setFormData({ ...formData, baseUnitId: e.target.value })}
                        >
                            <option value="">{t('common.none')} ({t('inventory.baseUnit')})</option>
                            {baseUnits.map(u => (
                                <option key={u.id} value={u.id}>{u.name} ({u.code})</option>
                            ))}
                        </select>
                        <p className="text-xs text-[hsl(var(--muted-foreground))]">
                            Leave empty if this is the Base Unit for this Type.
                        </p>
                    </div>

                    {formData.baseUnitId && (
                        <div className="space-y-2">
                            <label className="text-sm font-medium">{t('inventory.factor')}</label>
                            <div className="flex items-center gap-2">
                                <span className="text-sm">1 {formData.code || 'Unit'} = </span>
                                <input
                                    type="number"
                                    step="0.0001"
                                    className="w-24 h-8 px-2 rounded-md border border-[hsl(var(--input))] bg-[hsl(var(--background))]"
                                    value={formData.conversionFactor}
                                    onChange={(e) => setFormData({ ...formData, conversionFactor: parseFloat(e.target.value) || 0 })}
                                />
                                <span className="text-sm">
                                    {baseUnits.find(u => u.id === formData.baseUnitId)?.code || 'Base Unit'}
                                </span>
                            </div>
                        </div>
                    )}

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
