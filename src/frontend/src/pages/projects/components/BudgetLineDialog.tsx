import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Button, Input, Select } from '@/components/ui';
import { Modal } from '@/components/ui/Modal';
import type { CreateBudgetLineDto, UpdateBudgetLineDto, ProjectBudgetLineDto } from '@/types/project';
import { BudgetCategory } from '@/types/project';
import { unitOfMeasureService } from '@/services/unitOfMeasureService';
import type { UnitOfMeasureDto } from '@/services/unitOfMeasureService';

interface BudgetLineDialogProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (data: CreateBudgetLineDto | UpdateBudgetLineDto) => Promise<void>;
    initialData?: ProjectBudgetLineDto;
    projectId: string;
}

export const BudgetLineDialog = ({ isOpen, onClose, onSubmit, initialData, projectId }: BudgetLineDialogProps) => {
    const { t } = useTranslation();
    const [loading, setLoading] = useState(false);
    const [uoms, setUoms] = useState<UnitOfMeasureDto[]>([]);

    // Form State
    const [formData, setFormData] = useState<Partial<CreateBudgetLineDto>>({
        projectId,
        costCode: '',
        description: '',
        quantity: 0,
        unitOfMeasureId: '',
        unitPrice: 0,
        category: BudgetCategory.Material
    });

    useEffect(() => {
        if (isOpen) {
            loadUoms();
            if (initialData) {
                setFormData({
                    costCode: initialData.costCode,
                    description: initialData.description,
                    quantity: initialData.quantity,
                    unitOfMeasureId: initialData.unitOfMeasureId,
                    unitPrice: initialData.unitPrice,
                    category: initialData.category
                });
            } else {
                setFormData({
                    projectId,
                    costCode: '',
                    description: '',
                    quantity: 0,
                    unitOfMeasureId: '',
                    unitPrice: 0,
                    category: BudgetCategory.Material
                });
            }
        }
    }, [isOpen, initialData, projectId]);

    const loadUoms = async () => {
        try {
            const response = await unitOfMeasureService.getAll();
            if (response.success && response.data) {
                setUoms(response.data);
            }
        } catch (error) {
            console.error('Failed to load UOMs', error);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();
        setLoading(true);
        try {
            await onSubmit(formData as CreateBudgetLineDto);
            onClose();
        } catch (error) {
            console.error(error);
        } finally {
            setLoading(false);
        }
    };

    const categoryOptions = [
        { value: '0', label: t('projects.financials.material') },
        { value: '1', label: t('projects.financials.labor') },
        { value: '2', label: t('projects.financials.subcontractor') },
        { value: '3', label: t('projects.financials.expense') },
        { value: '4', label: t('projects.financials.equipment') },
        { value: '5', label: t('projects.financials.overhead') },
    ];

    const uomOptions = uoms.map(u => ({ value: u.id, label: u.code }));

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={initialData ? t('projects.financials.editBudgetLine') : t('projects.financials.addBudgetLine')}
        >
            <form onSubmit={handleSubmit} className="space-y-4">
                <Input
                    label={t('common.code')}
                    value={formData.costCode}
                    onChange={(e) => setFormData({ ...formData, costCode: e.target.value })}
                    required
                />

                <Input
                    label={t('common.description')}
                    value={formData.description}
                    onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    required
                />

                <div className="grid grid-cols-2 gap-4">
                    <Input
                        label={t('common.quantity')}
                        type="number"
                        step="0.01"
                        value={formData.quantity}
                        onChange={(e) => setFormData({ ...formData, quantity: parseFloat(e.target.value) })}
                        required
                    />

                    <Select
                        label={t('common.unit')}
                        options={uomOptions}
                        value={formData.unitOfMeasureId}
                        onChange={(e) => setFormData({ ...formData, unitOfMeasureId: e.target.value })}
                        required
                    />
                </div>

                <div className="grid grid-cols-2 gap-4">
                    <Input
                        label={t('projects.financials.unitPrice')}
                        type="number"
                        step="0.01"
                        value={formData.unitPrice}
                        onChange={(e) => setFormData({ ...formData, unitPrice: parseFloat(e.target.value) })}
                        required
                    />

                    <Select
                        label={t('common.category')}
                        options={categoryOptions}
                        value={formData.category?.toString()}
                        onChange={(e) => setFormData({ ...formData, category: parseInt(e.target.value) as BudgetCategory })}
                        required
                    />
                </div>

                <div className="text-right text-sm font-medium pt-2">
                    Total: {((formData.quantity || 0) * (formData.unitPrice || 0)).toLocaleString()}
                </div>

                <div className="flex justify-end gap-2 pt-4">
                    <Button type="button" variant="ghost" onClick={onClose}>
                        {t('common.cancel')}
                    </Button>
                    <Button type="submit" isLoading={loading}>
                        {loading ? t('common.saving') : t('common.save')}
                    </Button>
                </div>
            </form>
        </Modal>
    );
};
