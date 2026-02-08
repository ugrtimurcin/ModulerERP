import { useState, useEffect } from 'react';
import type { ProgressPaymentDto, CreateProgressPaymentDto } from '@/types/project';
import { projectService } from '@/services/projectService';
import { Modal } from '@/components/ui/Modal';
import { Button, Input } from '@/components/ui';
import { Plus, Check, FileText } from 'lucide-react';
import { useDialog } from '@/components/ui/Dialog';

interface PaymentsTabProps {
    projectId: string;
}

export function PaymentsTab({ projectId }: PaymentsTabProps) {
    const [loading, setLoading] = useState(true);
    const [payments, setPayments] = useState<ProgressPaymentDto[]>([]);
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const { confirm } = useDialog();

    // Form inputs
    const [createForm, setCreateForm] = useState<Partial<CreateProgressPaymentDto>>({
        date: new Date().toISOString().split('T')[0],
        currentAmount: 0,
        retentionRate: 10 // Default 10%
    });

    useEffect(() => {
        loadPayments();
    }, [projectId]);

    const loadPayments = async () => {
        try {
            const response = await projectService.payments.getByProject(projectId);
            if (response.success && response.data) {
                setPayments(response.data);
            }
        } catch (error) {
            console.error('Failed to load payments', error);
        } finally {
            setLoading(false);
        }
    };

    const handleCreate = async () => {
        try {
            if (!createForm.currentAmount || !createForm.date) return;

            await projectService.payments.create({
                projectId,
                date: createForm.date,
                currentAmount: typeof createForm.currentAmount === 'string' ? parseFloat(createForm.currentAmount) : createForm.currentAmount,
                retentionRate: typeof createForm.retentionRate === 'string' ? parseFloat(createForm.retentionRate) : createForm.retentionRate
            } as CreateProgressPaymentDto);

            setIsCreateModalOpen(false);
            loadPayments();
            setCreateForm({ date: new Date().toISOString().split('T')[0], currentAmount: 0, retentionRate: 10 });
        } catch (error) {
            console.error('Failed to create payment', error);
        }
    };

    const handleApprove = async (id: string, paymentNo: number) => {
        confirm({
            title: `Approve Payment #${paymentNo}`,
            message: 'Are you sure? This will generate invoices and financial records.',
            confirmText: 'Approve',
            onConfirm: async () => {
                try {
                    await projectService.payments.approve(id);
                    loadPayments();
                } catch (error) {
                    console.error('Failed to approve', error);
                }
            }
        });
    };

    if (loading) return <div>Loading payments...</div>;

    const formatCurrency = (val: number) => new Intl.NumberFormat('tr-TR', { style: 'currency', currency: 'TRY' }).format(val); // Assuming TRY for now or fetch project currency

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium">Progress Payments (Hakediş)</h3>
                <Button onClick={() => setIsCreateModalOpen(true)}>
                    <Plus className="mr-2 h-4 w-4" />
                    Create Hakediş
                </Button>
            </div>

            <div className="rounded-md border">
                <table className="w-full text-sm">
                    <thead>
                        <tr className="border-b bg-muted/50">
                            <th className="p-3 text-left">No</th>
                            <th className="p-3 text-left">Date</th>
                            <th className="p-3 text-right">Previous Cumulative</th>
                            <th className="p-3 text-right">Current Amount</th>
                            <th className="p-3 text-right">Retention</th>
                            <th className="p-3 text-right">Net Payable</th>
                            <th className="p-3 text-center">Status</th>
                            <th className="p-3 text-right">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        {payments.length === 0 ? (
                            <tr>
                                <td colSpan={8} className="p-4 text-center text-muted-foreground">
                                    No payments found.
                                </td>
                            </tr>
                        ) : (
                            payments.map(payment => (
                                <tr key={payment.id} className="border-b last:border-0 hover:bg-muted/50">
                                    <td className="p-3 font-medium">#{payment.paymentNo}</td>
                                    <td className="p-3">{new Date(payment.date).toLocaleDateString()}</td>
                                    <td className="p-3 text-right text-muted-foreground">{formatCurrency(payment.previousCumulativeAmount)}</td>
                                    <td className="p-3 text-right font-medium">{formatCurrency(payment.currentAmount)}</td>
                                    <td className="p-3 text-right text-red-500">-{formatCurrency(payment.retentionAmount)}</td>
                                    <td className="p-3 text-right font-bold text-green-600">{formatCurrency(payment.netPayableAmount)}</td>
                                    <td className="p-3 text-center">
                                        <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${payment.status === 0 ? 'bg-yellow-100 text-yellow-800' :
                                            payment.status === 1 ? 'bg-green-100 text-green-800' :
                                                'bg-blue-100 text-blue-800'
                                            }`}>
                                            {payment.status === 0 ? 'Draft' : payment.status === 1 ? 'Approved' : 'Invoiced'}
                                        </span>
                                    </td>
                                    <td className="p-3 text-right">
                                        {payment.status === 0 && (
                                            <Button variant="ghost" size="sm" onClick={() => handleApprove(payment.id, payment.paymentNo)}>
                                                <Check className="h-4 w-4 mr-1" /> Approve
                                            </Button>
                                        )}
                                        {payment.status === 1 && (
                                            <Button variant="ghost" size="sm" disabled>
                                                <FileText className="h-4 w-4 mr-1" /> Invoice
                                            </Button>
                                        )}
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            <Modal
                isOpen={isCreateModalOpen}
                onClose={() => setIsCreateModalOpen(false)}
                title="Create Progress Payment"
            >
                <div className="space-y-4 py-4">
                    <Input
                        label="Date"
                        type="date"
                        value={createForm.date}
                        onChange={(e) => setCreateForm({ ...createForm, date: e.target.value })}
                    />
                    <Input
                        label="Current Amount"
                        type="number"
                        value={createForm.currentAmount}
                        onChange={(e) => setCreateForm({ ...createForm, currentAmount: parseFloat(e.target.value) })}
                    />
                    <Input
                        label="Retention Rate (%)"
                        type="number"
                        value={createForm.retentionRate}
                        onChange={(e) => setCreateForm({ ...createForm, retentionRate: parseFloat(e.target.value) })}
                    />

                    {/* Read-Only info could be added here if we fetched 'Recent Cumulative' beforehand, but backend handles it */}

                    <div className="flex justify-end space-x-2 mt-4">
                        <Button variant="ghost" onClick={() => setIsCreateModalOpen(false)}>Cancel</Button>
                        <Button onClick={handleCreate}>Create</Button>
                    </div>
                </div>
            </Modal>
        </div>
    );
}
