import { useRef, useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import QRCode from 'react-qr-code';
import { Button } from '@/components/ui';
import { X, Printer } from 'lucide-react';
import { api } from '@/lib/api';

interface EmployeeQRDialogProps {
    open: boolean;
    onClose: () => void;
    employee: { id: string; firstName: string; lastName: string; jobTitle: string; qrToken?: string } | null;
}

export function EmployeeQRDialog({ open, onClose, employee }: EmployeeQRDialogProps) {
    const { t } = useTranslation();
    const printRef = useRef<HTMLDivElement>(null);
    const [token, setToken] = useState<string | null>(null);
    const [localEmp, setLocalEmp] = useState<any>(null);

    useEffect(() => {
        if (employee) {
            setLocalEmp(employee);
            setToken(employee.qrToken || null);
        }
    }, [employee]);

    if (!open || !localEmp) return null;

    const handleGenerate = async () => {
        try {
            const data = await api.put<{ token: string }>(`/hr/employees/${localEmp.id}/generate-qr`, {});
            setToken(data.token);
            // Also update local state or trigger parent refresh if needed
        } catch {
            // Handle error
        }
    };

    const handlePrint = () => {
        const printWindow = window.open('', '_blank');
        if (printWindow && printRef.current) {
            printWindow.document.write('<html><head><title>Print QR</title></head><body style="display:flex; flex-direction:column; align-items:center; justify-content:center; height:100vh;">');
            printWindow.document.write(printRef.current.innerHTML);
            printWindow.document.write('</body></html>');
            printWindow.document.close();
            printWindow.print();
        }
    };

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            <div className="absolute inset-0 bg-black/50 backdrop-blur-sm" onClick={onClose} />
            <div className="relative bg-[hsl(var(--card))] rounded-xl shadow-2xl w-full max-w-sm border border-[hsl(var(--border))]">
                <div className="px-6 py-4 border-b border-[hsl(var(--border))] flex items-center justify-between">
                    <h2 className="text-xl font-semibold">{t('hr.employeeQR')}</h2>
                    <button onClick={onClose} className="p-2 rounded-lg hover:bg-[hsl(var(--accent))]">
                        <X className="w-5 h-5" />
                    </button>
                </div>

                <div className="p-8 flex flex-col items-center gap-6">
                    <div ref={printRef} className="flex flex-col items-center p-4 bg-white rounded-xl shadow-sm w-full">
                        <div className="p-4 bg-white text-center w-full">
                            <h3 className="text-lg font-bold text-black mb-2">{localEmp.firstName} {localEmp.lastName}</h3>
                            <p className="text-sm text-gray-500 mb-4">{localEmp.jobTitle}</p>

                            {token ? (
                                <QRCode
                                    id="employee-qr-code"
                                    value={token}
                                    size={200}
                                    style={{ height: "auto", maxWidth: "100%", width: "100%" }}
                                    viewBox={`0 0 256 256`}
                                />
                            ) : (
                                <div className="h-48 flex items-center justify-center bg-gray-100 rounded-lg border-2 border-dashed border-gray-300">
                                    <p className="text-sm text-gray-400">No QR Token</p>
                                </div>
                            )}

                            <p className="text-[10px] text-gray-400 mt-2">{localEmp.id}</p>
                        </div>
                    </div>

                    <div className="flex flex-col w-full gap-3 pt-4">
                        {!token ? (
                            <Button className="w-full" onClick={handleGenerate}>
                                {t('hr.generateQR')}
                            </Button>
                        ) : (
                            <div className="flex gap-2">
                                <Button className="flex-1" variant="secondary" onClick={handlePrint}>
                                    <Printer className="w-4 h-4 mr-2" />
                                    {t('common.print')}
                                </Button>
                                {/* Download logic simplified by reusing Print or similar */}
                                <Button className="flex-1" variant="secondary" onClick={() => handlePrint()}>
                                    {t('common.download')}
                                </Button>
                            </div>
                        )}
                        <Button className="w-full" onClick={onClose}>
                            {t('common.close')}
                        </Button>
                    </div>
                </div>
            </div>
        </div>
    );
}
