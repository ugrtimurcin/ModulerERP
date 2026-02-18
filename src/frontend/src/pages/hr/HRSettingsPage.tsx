
import { useTranslation } from 'react-i18next';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui';
import { TaxRulesPage } from './TaxRulesPage';
import { SocialSecurityRulesPage } from './SocialSecurityRulesPage';
import { CommissionRulesPage } from './CommissionRulesPage';

export function HRSettingsPage() {
    const { t } = useTranslation();

    return (
        <div className="space-y-6">
            <div className="flex items-center justify-between">
                <div>
                    <h1 className="text-2xl font-bold">{t('nav.settings')}</h1>
                    <p className="text-[hsl(var(--muted-foreground))]">{t('hr.settingsSubtitle')}</p>
                </div>
            </div>

            <Tabs defaultValue="tax-rules" className="w-full">
                <TabsList className="mb-4">
                    <TabsTrigger value="tax-rules">{t('nav.taxRules')}</TabsTrigger>
                    <TabsTrigger value="social-security-rules">{t('nav.socialSecurityRules')}</TabsTrigger>
                    <TabsTrigger value="commission-rules">{t('nav.commissionRules')}</TabsTrigger>
                </TabsList>

                <TabsContent value="tax-rules">
                    <div className="border rounded-lg p-6 bg-[hsl(var(--card))]">
                        <TaxRulesPage />
                    </div>
                </TabsContent>

                <TabsContent value="social-security-rules">
                    <div className="border rounded-lg p-6 bg-[hsl(var(--card))]">
                        <SocialSecurityRulesPage />
                    </div>
                </TabsContent>

                <TabsContent value="commission-rules">
                    <div className="border rounded-lg p-6 bg-[hsl(var(--card))]">
                        <CommissionRulesPage />
                    </div>
                </TabsContent>
            </Tabs>
        </div>
    );
}
