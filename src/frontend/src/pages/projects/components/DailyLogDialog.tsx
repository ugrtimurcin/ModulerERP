import { useEffect, useState } from 'react';
import { useForm, useFieldArray } from 'react-hook-form';
import { useTranslation } from 'react-i18next';
import { Modal } from '@/components/ui/Modal';
import { Input, Select, Button } from '@/components/ui/Form';
import { Plus, Trash2 } from 'lucide-react';
import type { CreateDailyLogDto, ProjectResourceDto } from '@/types/project';
import { projectResourceService } from '@/services/projectResourceService';

interface DailyLogDialogProps {
    isOpen: boolean;
    onClose: () => void;
    onSubmit: (data: CreateDailyLogDto) => Promise<void>;
    projectId: string;
}

export function DailyLogDialog({ isOpen, onClose, onSubmit, projectId }: DailyLogDialogProps) {
    const { t } = useTranslation();
    const [resources, setResources] = useState<ProjectResourceDto[]>([]);

    const { register, control, handleSubmit, formState: { errors }, reset } = useForm<CreateDailyLogDto>({
        defaultValues: {
            projectId,
            date: new Date().toISOString().split('T')[0],
            weatherCondition: '',
            siteManagerNote: '',
            resourceUsages: [],
            materialUsages: []
        }
    });

    const { fields: resourceFields, append: appendResource, remove: removeResource } = useFieldArray({
        control,
        name: "resourceUsages"
    });

    // Material logic commented out until product picker is ready
    /*
    const { fields: materialFields, append: appendMaterial, remove: removeMaterial } = useFieldArray({
        control,
        name: "materialUsages"
    });
    */

    useEffect(() => {
        if (isOpen) {
            loadResources();
        }
    }, [isOpen, projectId]);

    const loadResources = async () => {
        try {
            const res = await projectResourceService.getByProject(projectId);
            if (res.success && res.data) {
                setResources(res.data);
            }
        } catch (error) {
            console.error('Failed to load resources', error);
        }
    };

    const handleFormSubmit = async (data: CreateDailyLogDto) => {
        await onSubmit({
            ...data,
            projectId,
            // Ensure numbers are numbers
            resourceUsages: data.resourceUsages.map(r => ({
                ...r,
                hoursWorked: Number(r.hoursWorked)
            })),
            materialUsages: data.materialUsages.map(m => ({
                ...m,
                quantity: Number(m.quantity)
            }))
        });
        reset();
    };

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={t('projects.dailyLogs.addDailyLog')}
            size="xl"
        >
            <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-6">

                {/* Header Info */}
                <div className="grid grid-cols-2 gap-4">
                    <Input
                        label={t('common.date')}
                        type="date"
                        {...register('date', { required: t('validation.required') })}
                        error={errors.date?.message}
                    />
                    <Input
                        label={t('projects.dailyLogs.weatherCondition')}
                        placeholder={t('projects.dailyLogs.weatherPlaceholder')}
                        {...register('weatherCondition', { required: t('validation.required') })}
                        error={errors.weatherCondition?.message}
                    />
                </div>

                <div className="space-y-1">
                    <label className="block text-sm font-medium">
                        {t('projects.dailyLogs.siteManagerNote')}
                    </label>
                    <textarea
                        className="w-full px-3 py-2.5 rounded-xl border border-[hsl(var(--border))] bg-[hsl(var(--background))] focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
                        rows={3}
                        {...register('siteManagerNote')}
                    />
                </div>

                {/* Resources Section */}
                <div className="border rounded-xl p-4 space-y-4">
                    <div className="flex justify-between items-center">
                        <h3 className="font-medium">{t('projects.dailyLogs.resourceUsage')}</h3>
                        <Button type="button" variant="outline" size="sm" onClick={() => appendResource({ projectResourceId: '', hoursWorked: 8, description: '' })}>
                            <Plus className="h-4 w-4 mr-2" />
                            {t('common.add')}
                        </Button>
                    </div>

                    {resourceFields.length > 0 && (
                        <div className="space-y-3">
                            {resourceFields.map((field, index) => (
                                <div key={field.id} className="grid grid-cols-12 gap-2 items-start">
                                    <div className="col-span-5">
                                        <Select
                                            {...register(`resourceUsages.${index}.projectResourceId`, { required: true })}
                                            options={resources.map(r => ({
                                                value: r.id,
                                                label: `${r.role} - ${r.employeeName || r.assetName || 'Unknown'}`
                                            }))}
                                            placeholder={t('projects.dailyLogs.selectResource')}
                                        />
                                    </div>
                                    <div className="col-span-2">
                                        <Input
                                            type="number"
                                            step="0.5"
                                            placeholder={t('common.hours')}
                                            {...register(`resourceUsages.${index}.hoursWorked`, { required: true })}
                                        />
                                    </div>
                                    <div className="col-span-4">
                                        <Input
                                            placeholder={t('common.description')}
                                            {...register(`resourceUsages.${index}.description`)}
                                        />
                                    </div>
                                    <div className="col-span-1 flex justify-end">
                                        <Button type="button" variant="ghost" size="icon" onClick={() => removeResource(index)} className="text-red-500">
                                            <Trash2 className="h-4 w-4" />
                                        </Button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    )}
                </div>

                {/* Materials Section (Placeholder for now, simplified) */}
                <div className="border rounded-xl p-4 space-y-4">
                    <div className="flex justify-between items-center">
                        <h3 className="font-medium">{t('projects.dailyLogs.materialUsage')}</h3>
                        {/* 
                        <Button type="button" variant="outline" size="sm" onClick={() => appendMaterial({ productId: '', quantity: 1, unitOfMeasureId: '', location: '' })}>
                            <Plus className="h-4 w-4 mr-2" />
                            {t('common.add')}
                        </Button>
                        */}
                    </div>
                    <p className="text-xs text-muted-foreground">{t('common.featureUnderConstruction')} - Product Picker needed</p>
                </div>

                <div className="flex justify-end gap-3 pt-4 border-t">
                    <Button type="button" variant="ghost" onClick={onClose}>
                        {t('common.cancel')}
                    </Button>
                    <Button type="submit">
                        {t('common.save')}
                    </Button>
                </div>
            </form>
        </Modal>
    );
}
