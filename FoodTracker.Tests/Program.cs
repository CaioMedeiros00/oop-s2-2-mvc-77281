using Food.Domain.Models;

namespace Food.Tests;

class Program
{
    static int passedTests = 0;
    static int failedTests = 0;

    static void Main(string[] args)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine(" Food Safety Inspection Tracker - Test Suite");
        Console.WriteLine("═══════════════════════════════════════════════════════\n");

        RunTest("FollowUp_CannotBeClosedWithoutClosedDate", () =>
        {
            var followUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Now.AddDays(7),
                Status = FollowUpStatus.Closed,
                ClosedDate = null
            };

            var isValid = followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate.HasValue;

            AssertFalse(isValid, "Follow-up should not be valid when closed without a closed date");
        });

        RunTest("FollowUp_ValidWhenClosedWithClosedDate", () =>
        {
            var followUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Now.AddDays(7),
                Status = FollowUpStatus.Closed,
                ClosedDate = DateTime.Now
            };

            var isValid = followUp.Status == FollowUpStatus.Closed && followUp.ClosedDate.HasValue;

            AssertTrue(isValid, "Follow-up should be valid when closed with a closed date");
        });

        RunTest("FollowUp_IsOverdue_WhenDueDateIsPast", () =>
        {
            var followUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Now.AddDays(-5),
                Status = FollowUpStatus.Open
            };

            var isOverdue = followUp.Status == FollowUpStatus.Open && followUp.DueDate < DateTime.Now;

            AssertTrue(isOverdue, "Follow-up should be overdue when due date is in the past and status is Open");
        });

        RunTest("FollowUp_IsNotOverdue_WhenDueDateIsFuture", () =>
        {
            var followUp = new FollowUp
            {
                InspectionId = 1,
                DueDate = DateTime.Now.AddDays(5),
                Status = FollowUpStatus.Open
            };

            var isOverdue = followUp.Status == FollowUpStatus.Open && followUp.DueDate < DateTime.Now;

            AssertFalse(isOverdue, "Follow-up should not be overdue when due date is in the future");
        });

        RunTest("Inspection_ScoreIsWithinValidRange_Score0", () =>
        {
            var inspection = new Inspection
            {
                PremisesId = 1,
                InspectionDate = DateTime.Now,
                Score = 0,
                Outcome = InspectionOutcome.Fail
            };

            AssertTrue(inspection.Score >= 0 && inspection.Score <= 100, "Score 0 should be valid");
        });

        RunTest("Inspection_ScoreIsWithinValidRange_Score50", () =>
        {
            var inspection = new Inspection
            {
                PremisesId = 1,
                InspectionDate = DateTime.Now,
                Score = 50,
                Outcome = InspectionOutcome.Fail
            };

            AssertTrue(inspection.Score >= 0 && inspection.Score <= 100, "Score 50 should be valid");
        });

        RunTest("Inspection_ScoreIsWithinValidRange_Score100", () =>
        {
            var inspection = new Inspection
            {
                PremisesId = 1,
                InspectionDate = DateTime.Now,
                Score = 100,
                Outcome = InspectionOutcome.Pass
            };

            AssertTrue(inspection.Score >= 0 && inspection.Score <= 100, "Score 100 should be valid");
        });

        RunTest("Inspection_PassOutcome_WhenScoreIs70OrMore", () =>
        {
            var inspection = new Inspection
            {
                PremisesId = 1,
                InspectionDate = DateTime.Now,
                Score = 85,
                Outcome = InspectionOutcome.Pass
            };

            AssertEqual(InspectionOutcome.Pass, inspection.Outcome, "Inspection with score 85 should have Pass outcome");
        });

        RunTest("Inspection_FailOutcome_WhenScoreBelow70", () =>
        {
            var inspection = new Inspection
            {
                PremisesId = 1,
                InspectionDate = DateTime.Now,
                Score = 60,
                Outcome = InspectionOutcome.Fail
            };

            AssertEqual(InspectionOutcome.Fail, inspection.Outcome, "Inspection with score 60 should have Fail outcome");
        });

        RunTest("Premises_RiskRating_CanBeHigh", () =>
        {
            var premises = new Premises
            {
                Name = "Test Restaurant",
                Address = "123 Main St",
                Town = "Auckland",
                RiskRating = RiskRating.High
            };

            AssertEqual(RiskRating.High, premises.RiskRating, "Premises should have High risk rating");
        });

        RunTest("Premises_RiskRating_CanBeMedium", () =>
        {
            var premises = new Premises
            {
                Name = "Test Cafe",
                Address = "456 Queen St",
                Town = "Wellington",
                RiskRating = RiskRating.Medium
            };

            AssertEqual(RiskRating.Medium, premises.RiskRating, "Premises should have Medium risk rating");
        });

        RunTest("Premises_RiskRating_CanBeLow", () =>
        {
            var premises = new Premises
            {
                Name = "Test Bakery",
                Address = "789 High St",
                Town = "Christchurch",
                RiskRating = RiskRating.Low
            };

            AssertEqual(RiskRating.Low, premises.RiskRating, "Premises should have Low risk rating");
        });

        Console.WriteLine("\n═══════════════════════════════════════════════════════");
        Console.WriteLine($" Test Results: {passedTests} passed, {failedTests} failed");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        Environment.Exit(failedTests > 0 ? 1 : 0);
    }

    static void RunTest(string testName, Action test)
    {
        Console.Write($"[TEST] {testName,-50} ");
        try
        {
            test();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ PASSED");
            Console.ResetColor();
            passedTests++;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ FAILED");
            Console.ResetColor();
            Console.WriteLine($"       {ex.Message}");
            failedTests++;
        }
    }

    static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new Exception($"{message}. Expected: {expected}, Actual: {actual}");
        }
    }

    static void AssertTrue(bool condition, string message)
    {
        if (!condition)
        {
            throw new Exception(message);
        }
    }

    static void AssertFalse(bool condition, string message)
    {
        if (condition)
        {
            throw new Exception(message);
        }
    }
}
