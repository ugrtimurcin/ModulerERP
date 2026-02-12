import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { Modal } from '@/components/ui/Modal';
import { Input, Select } from '@/components/ui/Form';
import type { CreateBoQItemDto, UpdateBoQItemDto, BillOfQuantitiesItemDto } from '@/types/project';
import { BudgetCategory } from '@/types/project';
import { useUnitOfMeasures } from '@/services/unitOfMeasureService';

interface BoQItemDialogProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (data: CreateBoQItemDto | UpdateBoQItemDto) => Promise<void>;
    initialData?: BillOfQuantitiesItemDto;
    parentId?: string;
    projectId: string;
}

export function BoQItemDialog({
    isOpen,
    onClose,
    onSubmit,
    initialData,
    parentId,
    projectId
}: BoQItemDialogProps) {
    const { t } = useTranslation();
    const { units } = useUnitOfMeasures();

    const { register, handleSubmit, reset, formState: { errors, isSubmitting } } = useForm<CreateBoQItemDto | UpdateBoQItemDto>({
        defaultValues: {
            projectId,
            parentId,
            itemCode: '',
            description: '',
            quantity: 0,
            unitOfMeasureId: '',
            contractUnitPrice: 0,
            estimatedUnitCost: 0,
            category: BudgetCategory.Material
        }
    });

    useEffect(() => {
        if (isOpen) {
            if (initialData) {
                reset({
                    itemCode: initialData.itemCode,
                    description: initialData.description,
                    quantity: initialData.quantity,
                    unitOfMeasureId: initialData.unitOfMeasureId,
                    contractUnitPrice: initialData.contractUnitPrice,
                    estimatedUnitCost: initialData.estimatedUnitCost,
                    category: initialData.category
                });
            } else {
                reset({
                    projectId,
                    parentId,
                    itemCode: '',
                    description: '',
                    quantity: 1, // Default to 1
                    unitOfMeasureId: '',
                    contractUnitPrice: 0,
                    estimatedUnitCost: 0,
                    category: BudgetCategory.Material
                });
            }
        }
    }, [isOpen, initialData, parentId, projectId, reset]);

    const budgetCategories = [
        { value: BudgetCategory.Material, label: t('budget.categories.material') },
        { value: BudgetCategory.Labor, label: t('budget.categories.labor') },
        { value: BudgetCategory.Subcontractor, label: t('budget.categories.subcontractor') },
        { value: BudgetCategory.Equipment, label: t('budget.categories.equipment') },
        { value: BudgetCategory.Expense, label: t('budget.categories.expense') },
        { value: BudgetCategory.Overhead, label: t('budget.categories.overhead') },
    ];

    // Helper options for Select
    const unitOptions = units.map(u => ({ value: u.id, label: u.code }));
    const categoryOptions = budgetCategories.map(c => ({ value: c.value.toString(), label: c.label }));

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={initialData ? t('projects.boq.editItem') : t('projects.boq.addItem')}
        >
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <Input
                    label={t('projects.boq.itemCode')}
                    error={errors.itemCode?.message}
                    {...register('itemCode', { required: t('validation.required') })}
                />

                <div className="space-y-1">
                    <label className="block text-sm font-medium">
                        {t('common.description')} <span className="text-red-500 ml-1">*</span>
                    </label>
                    <textarea
                        className="w-full px-3 py-2.5 rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                        rows={3}
                        {...register('description', { required: t('validation.required') })}
                    />
                    {errors.description?.message && <p className="text-sm text-red-500">{errors.description.message}</p>}
                </div>

                <div className="grid grid-cols-2 gap-4">
                    <Input
                        type="number"
                        label={t('common.quantity')}
                        step="0.01"
                        error={errors.quantity?.message}
                        {...register('quantity', {
                            required: t('validation.required'),
                            valueAsNumber: true,
                            min: { value: 0.0001, message: t('validation.positive') }
                        })}
                    />

                    <Select
                        label={t('common.unit')}
                        options={unitOptions}
                        error={errors.unitOfMeasureId?.message}
                        {...register('unitOfMeasureId', { required: t('validation.required') })}
                    />
                </div>

                <div className="grid grid-cols-2 gap-4">
                    <Input
                        type="number"
                        label={t('projects.boq.unitPrice')} // Income
                        step="0.01"
                        error={errors.contractUnitPrice?.message}
                        {...register('contractUnitPrice', {
                            required: t('validation.required'),
                            valueAsNumber: true,
                            min: 0
                        })}
                    />

                    <Input
                        type="number"
                        label={t('projects.boq.unitCost')} // Expense
                        step="0.01"
                        error={errors.estimatedUnitCost?.message}
                        {...register('estimatedUnitCost', {
                            required: t('validation.required'),
                            valueAsNumber: true,
                            min: 0
                        })}
                    />
                </div>

                <Select
                    label={t('budget.category')}
                    options={categoryOptions}
                    error={errors.category?.message}
                    {...register('category', { required: t('validation.required') })}
                />

                <div className="flex justify-end space-x-2 pt-4">
                    <button
                        type="button"
                        onClick={onClose}
                        className="px-4 py-2 text-sm font-medium text-gray-700 bg-white border border-gray-300 rounded-md hover:bg-gray-50"
                    >
                        {t('common.cancel')}
                    </button>
                    <button
                        type="submit"
                        disabled={isSubmitting}
                        className="px-4 py-2 text-sm font-medium text-white bg-indigo-600 rounded-md hover:bg-indigo-700 disabled:opacity-50"
                    >
                        {isSubmitting ? t('common.saving') : t('common.save')}
                    </button>
                </div>
            </form>
        </Modal>
    );
}
