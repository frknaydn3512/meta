using AdReport.Domain.Enums;

namespace AdReport.Domain.Extensions;

public static class PlanExtensions
{
    public static int GetMaxClientsLimit(this PlanType plan)
    {
        return plan switch
        {
            PlanType.Starter => 3,
            PlanType.Agency => 15,
            PlanType.Scale => int.MaxValue,
            _ => 0
        };
    }
}