import { useState, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { Upload, Trash, Download, FileText } from 'lucide-react';
import { Button, Input } from '@/components/ui';
import { Modal } from '@/components/ui/Modal';
import { useDialog } from '@/components/ui/Dialog';
import { api } from '@/lib/api';
import type { ProjectDocumentDto, CreateProjectDocumentDto } from '@/types/project';

interface DocumentsTabProps {
    projectId: string;
}

export function DocumentsTab({ projectId }: DocumentsTabProps) {
    const { t } = useTranslation();
    const { confirm } = useDialog();
    const [loading, setLoading] = useState(true);
    const [documents, setDocuments] = useState<ProjectDocumentDto[]>([]);
    const [isModalOpen, setIsModalOpen] = useState(false);

    const [formData, setFormData] = useState<Partial<CreateProjectDocumentDto>>({
        title: '',
        documentType: 'Contract',
        description: '',
        fileUrl: '' // Phantom file for now
    });

    useEffect(() => {
        loadDocuments();
    }, [projectId]);

    const loadDocuments = async () => {
        try {
            const response = await api.get<{ data: ProjectDocumentDto[] }>(`/projectdocuments/project/${projectId}`);
            if (response.data) {
                setDocuments(response.data);
            }
        } catch (error) {
            console.error('Failed to load documents', error);
        } finally {
            setLoading(false);
        }
    };

    const handleDelete = (doc: ProjectDocumentDto) => {
        confirm({
            title: t('common.delete'),
            message: t('common.confirmDelete'), // "Are you sure you want to delete this item?"
            confirmText: t('common.delete'),
            onConfirm: async () => {
                try {
                    await api.delete(`/projectdocuments/${doc.id}`);
                    loadDocuments();
                } catch (error) {
                    console.error('Failed to delete document', error);
                }
            }
        });
    };

    const handleSubmit = async () => {
        try {
            if (!formData.title || !formData.documentType) return;

            // Mock File Upload (Generate a fake URL or ID)
            const fakeFileId = crypto.randomUUID();
            const fakeUrl = `/uploads/${fakeFileId}.pdf`;

            await api.post('/projectdocuments', {
                projectId,
                title: formData.title,
                documentType: formData.documentType,
                description: formData.description || '',
                fileUrl: fakeUrl,
                systemFileId: undefined // In real app, upload first then get ID
            });

            setIsModalOpen(false);
            loadDocuments();
            setFormData({ title: '', documentType: 'Contract', description: '', fileUrl: '' });
        } catch (error) {
            console.error('Failed to upload document', error);
        }
    };

    if (loading) return <div>{t('common.loading')}</div>;

    return (
        <div className="space-y-4">
            <div className="flex justify-between items-center">
                <h3 className="text-lg font-medium">{t('projects.tabs.documents')}</h3>
                <Button onClick={() => setIsModalOpen(true)}>
                    <Upload className="mr-2 h-4 w-4" />
                    {t('projects.documents.upload')}
                </Button>
            </div>

            <div className="rounded-md border bg-card">
                <table className="w-full text-sm">
                    <thead>
                        <tr className="border-b bg-muted/50">
                            <th className="p-3 text-left w-10"></th>
                            <th className="p-3 text-left">{t('projects.documents.title')}</th>
                            <th className="p-3 text-left">{t('projects.documents.type')}</th>
                            <th className="p-3 text-left">{t('projects.description')}</th>
                            <th className="p-3 text-right">{t('common.actions')}</th>
                        </tr>
                    </thead>
                    <tbody>
                        {documents.length === 0 ? (
                            <tr>
                                <td colSpan={5} className="p-8 text-center text-muted-foreground">
                                    {t('projects.documents.noDocuments')}
                                </td>
                            </tr>
                        ) : (
                            documents.map(doc => (
                                <tr key={doc.id} className="border-b last:border-0 hover:bg-muted/50">
                                    <td className="p-3 text-center">
                                        <FileText className="h-4 w-4 text-muted-foreground" />
                                    </td>
                                    <td className="p-3 font-medium">{doc.title}</td>
                                    <td className="p-3">
                                        <span className="inline-flex items-center rounded-full bg-blue-50 px-2 py-1 text-xs font-medium text-blue-700 ring-1 ring-inset ring-blue-700/10">
                                            {doc.documentType}
                                        </span>
                                    </td>
                                    <td className="p-3 text-muted-foreground">{doc.description}</td>
                                    <td className="p-3 text-right space-x-2">
                                        <Button variant="ghost" size="icon" className="h-8 w-8">
                                            <Download className="h-4 w-4" />
                                        </Button>
                                        <Button variant="ghost" size="icon" className="h-8 w-8 text-red-500 hover:text-red-600" onClick={() => handleDelete(doc)}>
                                            <Trash className="h-4 w-4" />
                                        </Button>
                                    </td>
                                </tr>
                            ))
                        )}
                    </tbody>
                </table>
            </div>

            <Modal
                isOpen={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                title={t('projects.documents.upload')}
            >
                <div className="space-y-4 py-4">
                    <Input
                        label={t('projects.documents.title')}
                        value={formData.title}
                        onChange={(e) => setFormData({ ...formData, title: e.target.value })}
                        placeholder="e.g. Contract Signed"
                    />

                    <div className="flex flex-col space-y-1.5">
                        <label className="text-sm font-medium">{t('projects.documents.type')}</label>
                        <select
                            className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background file:border-0 file:bg-transparent file:text-sm file:font-medium placeholder:text-muted-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-50"
                            value={formData.documentType}
                            onChange={(e) => setFormData({ ...formData, documentType: e.target.value })}
                        >
                            <option value="Contract">Contract</option>
                            <option value="Blueprint">Blueprint</option>
                            <option value="Invoice">Invoice</option>
                            <option value="Photo">Photo</option>
                            <option value="Report">Report</option>
                            <option value="Other">Other</option>
                        </select>
                    </div>

                    <Input
                        label={t('projects.description')}
                        value={formData.description}
                        onChange={(e) => setFormData({ ...formData, description: e.target.value })}
                    />

                    {/* Mock File Input */}
                    <div className="flex flex-col space-y-1.5">
                        <label className="text-sm font-medium">{t('projects.documents.file')}</label>
                        <input type="file" className="block w-full text-sm text-slate-500
                            file:mr-4 file:py-2 file:px-4
                            file:rounded-full file:border-0
                            file:text-sm file:font-semibold
                            file:bg-violet-50 file:text-violet-700
                            hover:file:bg-violet-100
                        "/>
                        <p className="text-xs text-muted-foreground">Uploading is mocked. A phantom file record will be created.</p>
                    </div>

                    <div className="flex justify-end space-x-2 mt-4">
                        <Button variant="ghost" onClick={() => setIsModalOpen(false)}>{t('common.cancel')}</Button>
                        <Button onClick={handleSubmit}>{t('common.save')}</Button>
                    </div>
                </div>
            </Modal>
        </div>
    );
}
