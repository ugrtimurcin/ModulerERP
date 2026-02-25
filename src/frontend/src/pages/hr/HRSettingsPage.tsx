
import { useTranslation } from 'react-i18next';
import { Tabs, TabsList, TabsTrigger, TabsContent } from '@/components/ui';
import { TaxRulesPage } from './TaxRulesPage';
import { SocialSecurityRulesPage } from './SocialSecurityRulesPage';
import { CommissionRulesPage } from './CommissionRulesPage';
import { LeavePoliciesPage } from './LeavePoliciesPage';
import { EarningDeductionTypesPage } from './EarningDeductionTypesPage';
import { SgkRiskProfilesPage } from './SgkRiskProfilesPage';

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
                <TabsList className="mb-4 flex flex-wrap gap-2 h-auto p-2 bg-[hsl(var(--muted)/0.5)] rounded-xl">
                    <TabsTrigger value="tax-rules" className="rounded-lg data-[state=active]:shadow-sm px-4 py-2">{t('nav.taxRules', 'Tax Rules')}</TabsTrigger>
                    <TabsTrigger value="social-security-rules" className="rounded-lg data-[state=active]:shadow-sm px-4 py-2">{t('nav.socialSecurityRules', 'Social Security Rules')}</TabsTrigger>
                    <TabsTrigger value="commission-rules" className="rounded-lg data-[state=active]:shadow-sm px-4 py-2">{t('nav.commissionRules', 'Commission Rules')}</TabsTrigger>
                    <TabsTrigger value="leave-policies" className="rounded-lg data-[state=active]:shadow-sm px-4 py-2">{t('hr.leavePolicies', 'Leave Policies')}</TabsTrigger>
                    <TabsTrigger value="earning-types" className="rounded-lg data-[state=active]:shadow-sm px-4 py-2">{t('hr.earningDeductionTypes', 'Earning & Deduction Types')}</TabsTrigger>
                    <TabsTrigger value="sgk-risk-profiles" className="rounded-lg data-[state=active]:shadow-sm px-4 py-2">{t('hr.sgkRiskProfiles', 'SGK Risk Profiles')}</TabsTrigger>
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

                <TabsContent value="leave-policies">
                    <div className="border rounded-lg p-6 bg-[hsl(var(--card))]">
                        <LeavePoliciesPage />
                    </div>
                </TabsContent>

                <TabsContent value="earning-types">
                    <div className="border rounded-lg p-6 bg-[hsl(var(--card))]">
                        <EarningDeductionTypesPage />
                    </div>
                </TabsContent>

                <TabsContent value="sgk-risk-profiles">
                    <div className="border rounded-lg p-6 bg-[hsl(var(--card))]">
                        <SgkRiskProfilesPage />
                    </div>
                </TabsContent>
            </Tabs>
        </div>
    );
}
