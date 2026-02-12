import { useState, useEffect, useCallback } from 'react';
import { useTranslation } from 'react-i18next';
import { Plus, Pencil, Trash2, Users, Mail, Building2, QrCode } from 'lucide-react';
import { DataTable, Button, Badge, useToast, useDialog } from '@/components/ui';
import type { Column } from '@/components/ui';
import { EmployeeDialog } from './EmployeeDialog';
import { EmployeeQRDialog } from './EmployeeQRDialog';
import { api } from '@/lib/api';

import type { Employee, Department } from '@/types/hr';

export function EmployeesPage() {
    const { t } = useTranslation();
    const toast = useToast();
    const dialog = useDialog();

    const [employees, setEmployees] = useState<Employee[]>([]);
    const [departments, setDepartments] = useState<Department[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [dialogOpen, setDialogOpen] = useState(false);
    const [qrDialogOpen, setQrDialogOpen] = useState(false);
    const [editingEmployee, setEditingEmployee] = useState<Employee | null>(null);
    const [selectedEmployeeForQR, setSelectedEmployeeForQR] = useState<Employee | null>(null);

    const loadData = useCallback(async () => {
        setIsLoading(true);
        try {
            const [empData, deptData] = await Promise.all([
                api.get<Employee[]>('/hr/employees'),
                api.get<Department[]>('/hr/departments')
            ]);

            setEmployees(empData);
            setDepartments(deptData);
        } catch (error) {
            toast.error(t('common.error'));
        }
        setIsLoading(false);
    }, [toast]);

    useEffect(() => {
        loadData();
    }, [loadData]);

    const handleCreate = () => {
        setEditingEmployee(null);
        setDialogOpen(true);
    };

    const handleEdit = (employee: Employee) => {
        setEditingEmployee(employee);
        setDialogOpen(true);
    };

    const handleViewQR = (employee: Employee) => {
        setSelectedEmployeeForQR(employee);
        setQrDialogOpen(true);
    };

    const handleDelete = async (employee: Employee) => {
        const confirmed = await dialog.danger({
            title: t('hr.deleteEmployee'),
            message: t('hr.confirmDeleteEmployee', { name: `${employee.firstName} ${employee.lastName}` }),
            confirmText: t('common.delete'),
        });

        if (confirmed) {
            try {
                await api.delete(`/hr/employees/${employee.id}`);
                toast.success(t('hr.employeeDeleted'));
                loadData();
            } catch {
                toast.error(t('common.error'));
            }
        }
    };

    const handleDialogClose = (saved: boolean) => {
        setDialogOpen(false);
        if (saved) loadData();
    };

    const getStatusBadge = (status: number) => {
        const variants: Record<number, { variant: 'success' | 'warning' | 'error' | 'default', label: string }> = {
            0: { variant: 'success', label: t('hr.statuses.active') },
            1: { variant: 'warning', label: t('hr.statuses.onLeave') },
            2: { variant: 'error', label: t('hr.statuses.terminated') },
            3: { variant: 'default', label: t('hr.statuses.suspended') },
        };
        const config = variants[status] || { variant: 'default', label: t('common.unknown') };
        return <Badge variant={config.variant}>{config.label}</Badge>;
    };

    const columns: Column<Employee>[] = [
        {
            key: 'name',
            header: t('common.name'),
            render: (emp) => (
                <div className="flex items-center gap-3">
                    <div className="w-10 h-10 rounded-full bg-gradient-to-br from-indigo-500 to-purple-500 flex items-center justify-center text-white font-semibold">
                        {emp.firstName[0]}{emp.lastName[0]}
                    </div>
                    <div>
                        <p className="font-medium">{emp.firstName} {emp.lastName}</p>
                        <p className="text-xs text-[hsl(var(--muted-foreground))] flex items-center gap-1">
                            <Mail className="w-3 h-3" /> {emp.email}
                        </p>
                    </div>
                </div>
            ),
        },
        {
            key: 'jobTitle',
            header: t('hr.jobTitle'),
            render: (emp) => (
                <div>
                    <p className="font-medium">{emp.jobTitle}</p>
                    <p className="text-xs text-[hsl(var(--muted-foreground))] flex items-center gap-1">
                        <Building2 className="w-3 h-3" /> {emp.departmentName}
                    </p>
                </div>
            ),
        },
        {
            key: 'identityNumber',
            header: t('hr.identityNumber'),
            render: (emp) => <span className="font-mono text-sm">{emp.identityNumber}</span>,
        },
        {
            key: 'currentSalary',
            header: t('hr.salary'),
            render: (emp) => (
                <span className="font-mono font-medium">
                    {emp.currentSalary?.toLocaleString('en-US', { minimumFractionDigits: 2 })}
                </span>
            ),
        },
        {
            key: 'status',
            header: t('common.status'),
            render: (emp) => getStatusBadge(emp.status),
        },
    ];

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold flex items-center gap-2">
                        <Users className="w-6 h-6" />
                        {t('hr.employees')}
                    </h1>
                    <p className="text-[hsl(var(--muted-foreground))]">
                        {t('hr.employeesSubtitle')}
                    </p>
                </div>
                <Button onClick={handleCreate}>
                    <Plus className="w-4 h-4" />
                    {t('hr.createEmployee')}
                </Button>
            </div>

            <DataTable
                data={employees}
                columns={columns}
                keyField="id"
                isLoading={isLoading}
                searchable
                searchPlaceholder={t('common.search')}
                actions={(emp) => (
                    <div className="flex items-center gap-1">
                        <button
                            onClick={() => handleViewQR(emp)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('hr.viewQR')}
                        >
                            <QrCode className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleEdit(emp)}
                            className="p-2 rounded-lg hover:bg-[hsl(var(--accent))] transition-colors"
                            title={t('common.edit')}
                        >
                            <Pencil className="w-4 h-4" />
                        </button>
                        <button
                            onClick={() => handleDelete(emp)}
                            className="p-2 rounded-lg hover:bg-red-100 dark:hover:bg-red-900/30 text-red-600 transition-colors"
                            title={t('common.delete')}
                        >
                            <Trash2 className="w-4 h-4" />
                        </button>
                    </div>
                )}
            />

            <EmployeeDialog
                open={dialogOpen}
                onClose={handleDialogClose}
                employee={editingEmployee}
                departments={departments}
                employees={employees}
            />

            <EmployeeQRDialog
                open={qrDialogOpen}
                onClose={() => setQrDialogOpen(false)}
                employee={selectedEmployeeForQR}
            />
        </div>
    );
}
