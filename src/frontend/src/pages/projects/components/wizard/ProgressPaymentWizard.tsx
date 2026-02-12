import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Modal } from '@/components/ui/Modal';
import { Button } from '@/components/ui';
import { api } from '@/lib/api';
import type {
    ProgressPaymentDto,
    CreateProgressPaymentDto,
} from '@/types/project';

import { Loader2, ArrowLeft, ArrowRight, Save } from 'lucide-react';
import { useToast } from '@/components/ui/Toast';
import { Step1BasicInfo } from './steps/Step1BasicInfo';
import { Step2Quantities } from './steps/Step2Quantities';
import { Step3Financials } from './steps/Step3Financials';
import { Step4Review } from './steps/Step4Review';

interface ProgressPaymentWizardProps {
    isOpen: boolean;
    onClose: () => void;
    projectId: string;
    payment?: ProgressPaymentDto;
    onSaved: () => void;
}

export function ProgressPaymentWizard({ isOpen, onClose, projectId, payment, onSaved }: ProgressPaymentWizardProps) {
    const { t } = useTranslation();
    const toast = useToast();
    const [step, setStep] = useState(1);
    const [loading, setLoading] = useState(false);
    const [currentPayment, setCurrentPayment] = useState<ProgressPaymentDto | null>(null);

    // Form State (Initialized from payment or defaults)
    const [formData, setFormData] = useState<Partial<CreateProgressPaymentDto>>({
        date: new Date().toISOString().split('T')[0],
        periodStart: new Date().toISOString().split('T')[0],
        periodEnd: new Date().toISOString().split('T')[0],
        materialOnSiteAmount: 0,
        advanceDeductionAmount: 0,
        isExpense: false
    });

    useEffect(() => {
        if (isOpen) {
            setStep(1);
            if (payment) {
                setCurrentPayment(payment);
                setFormData({
                    date: payment.date.split('T')[0],
                    periodStart: payment.periodStart.split('T')[0],
                    periodEnd: payment.periodEnd.split('T')[0],
                    materialOnSiteAmount: payment.materialOnSiteAmount,
                    advanceDeductionAmount: payment.advanceDeductionAmount,
                    isExpense: payment.isExpense
                });
            } else {
                setCurrentPayment(null);
                setFormData({
                    date: new Date().toISOString().split('T')[0],
                    periodStart: new Date().toISOString().split('T')[0],
                    periodEnd: new Date().toISOString().split('T')[0],
                    materialOnSiteAmount: 0,
                    advanceDeductionAmount: 0,
                    isExpense: false
                });
            }
        }
    }, [isOpen, payment]);

    const handleNext = async () => {
        if (step === 1) {
            // Save/Create Draft on Step 1 completion
            try {
                setLoading(true);
                let savedPayment: ProgressPaymentDto;

                if (currentPayment) {
                    await api.put(`/projects/${projectId}/payments/${currentPayment.id}`, formData);
                    // Refresh payment data
                    const res = await api.get<{ data: ProgressPaymentDto[] }>(`/projects/${projectId}/payments`);
                    savedPayment = res.data?.find(p => p.id === currentPayment.id) || currentPayment;
                } else {
                    const res = await api.post<{ data: ProgressPaymentDto }>(`/projects/${projectId}/payments`, { ...formData, projectId });
                    if (res.data) {
                        savedPayment = res.data;
                        onSaved(); // Notify parent list to refresh
                    } else {
                        throw new Error('Failed to create');
                    }
                }
                setCurrentPayment(savedPayment);
                setStep(2);
            } catch (error) {
                console.error('Failed to save step 1', error);
                toast.error(t('common.error'), t('common.errorSaving'));
            } finally {
                setLoading(false);
            }
        } else if (step === 2) {
            // Step 2 is Quantities. They are saved individually or in batch by the step component ideally, 
            // but we might want to refresh calculation here before Step 3
            // For now assume Step 2 component handles saving details.
            // We refresh payment to get updated calculations (Gross Amount etc)
            if (currentPayment) {
                setLoading(true);
                try {
                    // Force recalculation by calling update or just fetch? 
                    // Fetching is safer to get server-side calculated totals
                    const res = await api.get<{ data: ProgressPaymentDto[] }>(`/projects/${projectId}/payments`);
                    const updated = res.data?.find(p => p.id === currentPayment.id);
                    if (updated) setCurrentPayment(updated);
                    setStep(3);
                } catch (e) { console.error(e); }
                finally { setLoading(false); }
            }
        } else if (step === 3) {
            // Step 3 is Financials (Deductions). Save them.
            if (currentPayment) {
                setLoading(true);
                try {
                    await api.put(`/projects/${projectId}/payments/${currentPayment.id}`, formData);
                    // Refresh for Review step
                    const res = await api.get<{ data: ProgressPaymentDto[] }>(`/projects/${projectId}/payments`);
                    const updated = res.data?.find(p => p.id === currentPayment.id);
                    if (updated) setCurrentPayment(updated);
                    setStep(4);
                } catch (e) {
                    console.error(e);
                    toast.error(t('common.error'), t('common.errorSaving'));
                } finally { setLoading(false); }
            }
        }
    };

    const handleFinish = () => {
        onSaved();
        onClose();
    };

    const renderStep = () => {
        switch (step) {
            case 1:
                return <Step1BasicInfo data={formData} onChange={setFormData} />;
            case 2:
                // We pass currentPayment because Step 2 needs the ID AND the Details list
                // If currentPayment is null (shouldn't be after Step 1), we can't show it.
                return currentPayment ? <Step2Quantities payment={currentPayment} projectId={projectId} onRefresh={async () => {
                    // Callback to refresh payment data if needed
                    const res = await api.get<{ data: ProgressPaymentDto[] }>(`/projects/${projectId}/payments`);
                    const updated = res.data?.find(p => p.id === currentPayment.id);
                    if (updated) setCurrentPayment(updated);
                }} /> : <div>{t('common.loading')}</div>;
            case 3:
                return currentPayment ? <Step3Financials data={formData} onChange={setFormData} payment={currentPayment} /> : null;
            case 4:
                return currentPayment ? <Step4Review payment={currentPayment} /> : null;
            default:
                return null;
        }
    };

    return (
        <Modal
            isOpen={isOpen}
            onClose={onClose}
            title={payment ? t('projects.payments.editPayment') : t('projects.payments.createHakedis')}
            size="xl"
        >
            <div className="flex flex-col h-[80vh]">
                {/* Stepper Header */}
                <div className="flex justify-between mb-4 border-b pb-4">
                    {[1, 2, 3, 4].map(s => (
                        <div key={s} className={`flex items-center ${s === step ? 'text-primary font-bold' : 'text-muted-foreground'}`}>
                            <div className={`w-6 h-6 rounded-full flex items-center justify-center mr-2 text-xs border ${s === step ? 'border-primary bg-primary/10' : 'border-muted'}`}>
                                {s}
                            </div>
                            <span className="hidden sm:inline">
                                {s === 1 && t('projects.payments.wizard.basicInfo')}
                                {s === 2 && t('projects.payments.wizard.quantities')}
                                {s === 3 && t('projects.payments.wizard.financials')}
                                {s === 4 && t('projects.payments.wizard.review')}
                            </span>
                        </div>
                    ))}
                </div>

                {/* Content */}
                <div className="flex-1 overflow-auto p-1">
                    {renderStep()}
                </div>

                {/* Footer */}
                <div className="flex justify-between mt-4 pt-4 border-t">
                    <Button variant="outline" onClick={step === 1 ? onClose : () => setStep(step - 1)} disabled={loading}>
                        {step === 1 ? t('common.cancel') : <><ArrowLeft className="w-4 h-4 mr-2" /> {t('common.back')}</>}
                    </Button>

                    {step < 4 ? (
                        <Button onClick={handleNext} disabled={loading}>
                            {loading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
                            {step === 1 && !currentPayment ? t('common.createDraft') : t('common.next')}
                            <ArrowRight className="w-4 h-4 ml-2" />
                        </Button>
                    ) : (
                        <Button onClick={handleFinish} className="bg-green-600 hover:bg-green-700">
                            <Save className="w-4 h-4 mr-2" />
                            {t('common.finish')}
                        </Button>
                    )}
                </div>
            </div>
        </Modal>
    );
}
