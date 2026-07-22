using System;

namespace Infrastructure.Persistence.Context.Seeders;

/// <summary>
/// Single source of truth for deterministic GUIDs, test user credentials, 
/// and email addresses used across seeders and integration tests.
/// </summary>
public static class SeedConstants
{
    /// <summary>
    /// Default standard password for all operational test accounts (non-admin).
    /// </summary>
    public const string DefaultPassword = "GatewayPass123!";

    /// <summary>
    /// Default password for root system administration accounts.
    /// </summary>
    public const string AdminPassword = "AdminPassword123!";

    /// <summary>
    /// Deterministic accounts and profiles for internal and external reviewers.
    /// </summary>
    public static class Reviewers
    {
        public static readonly Guid LeadReviewerId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        public const string LeadReviewerEmail = "reviewer@academicgateway.com";

        public static readonly Guid SeniorCurriculumReviewerId = Guid.Parse("10000000-0000-0000-0000-000000000002");
        public const string SeniorCurriculumReviewerEmail = "reviewer.senior@academicgateway.com";

        public static readonly Guid IndustryTrackReviewerId = Guid.Parse("10000000-0000-0000-0000-000000000003");
        public const string IndustryTrackReviewerEmail = "reviewer.industry@academicgateway.com";
    }

    /// <summary>
    /// Deterministic accounts and profiles for specialized professor archetypes.
    /// </summary>
    public static class Professors
    {
        // 1. Computer Science / Distributed Systems
        public static readonly Guid AlanTuringId = Guid.Parse("20000000-0000-0000-0000-000000000001");
        public const string AlanTuringEmail = "professor@academicgateway.com";

        // 2. AI / Deep Learning (Supervision Capacity Reached Edge Case)
        public static readonly Guid GeoffreyHintonId = Guid.Parse("20000000-0000-0000-0000-000000000002");
        public const string GeoffreyHintonEmail = "prof.hinton@academicgateway.com";

        // 3. Cybersecurity & Cryptography
        public static readonly Guid AdaLovelaceId = Guid.Parse("20000000-0000-0000-0000-000000000003");
        public const string AdaLovelaceEmail = "prof.lovelace@academicgateway.com";

        // 4. Computer Engineering / IoT & Embedded
        public static readonly Guid ClaudeShannonId = Guid.Parse("20000000-0000-0000-0000-000000000004");
        public const string ClaudeShannonEmail = "prof.shannon@academicgateway.com";

        // 5. Software Engineering / SQA & Compilers
        public static readonly Guid GraceHopperId = Guid.Parse("20000000-0000-0000-0000-000000000005");
        public const string GraceHopperEmail = "prof.hopper@academicgateway.com";

        // 6. Game Development / 3D Graphics & Shaders
        public static readonly Guid JohnVonNeumannId = Guid.Parse("20000000-0000-0000-0000-000000000006");
        public const string JohnVonNeumannEmail = "prof.vonneumann@academicgateway.com";

        // 7. Bioinformatics & Healthcare IT
        public static readonly Guid RosalindFranklinId = Guid.Parse("20000000-0000-0000-0000-000000000007");
        public const string RosalindFranklinEmail = "prof.franklin@academicgateway.com";

        // 8. Quantum Computing
        public static readonly Guid RichardFeynmanId = Guid.Parse("20000000-0000-0000-0000-000000000008");
        public const string RichardFeynmanEmail = "prof.feynman@academicgateway.com";

        // 9. Minimal Bio / New Professor Edge Case
        public static readonly Guid MargaretHamiltonId = Guid.Parse("20000000-0000-0000-0000-000000000009");
        public const string MargaretHamiltonEmail = "prof.hamilton@academicgateway.com";
    }

    /// <summary>
    /// Deterministic accounts and profiles for student personas across tracks.
    /// </summary>
    public static class Students
    {
        // 1. Standard CS / Software Engineering Senior
        public static readonly Guid JaneDoeId = Guid.Parse("30000000-0000-0000-0000-000000000001");
        public const string JaneDoeEmail = "student@academicgateway.com";

        // 2. Full-Stack Web Development Track
        public static readonly Guid JohnSmithId = Guid.Parse("30000000-0000-0000-0000-000000000002");
        public const string JohnSmithEmail = "student.john@academicgateway.com";

        // 3. AI / Machine Learning Track
        public static readonly Guid AliceJohnsonId = Guid.Parse("30000000-0000-0000-0000-000000000003");
        public const string AliceJohnsonEmail = "student.alice@academicgateway.com";

        // 4. Cybersecurity Track
        public static readonly Guid BobWilliamsId = Guid.Parse("30000000-0000-0000-0000-000000000004");
        public const string BobWilliamsEmail = "student.bob@academicgateway.com";

        // 5. Computer Engineering / IoT Track
        public static readonly Guid CharlieBrownId = Guid.Parse("30000000-0000-0000-0000-000000000005");
        public const string CharlieBrownEmail = "student.charlie@academicgateway.com";

        // 6. Game Development Track
        public static readonly Guid EvaMartinezId = Guid.Parse("30000000-0000-0000-0000-000000000006");
        public const string EvaMartinezEmail = "student.eva@academicgateway.com";

        // 7. Bioinformatics Track
        public static readonly Guid DavidLeeId = Guid.Parse("30000000-0000-0000-0000-000000000007");
        public const string DavidLeeEmail = "student.david@academicgateway.com";

        // 8. Double Major / Heavy Skill Stack Edge Case
        public static readonly Guid SophiaChenId = Guid.Parse("30000000-0000-0000-0000-000000000008");
        public const string SophiaChenEmail = "student.sophia@academicgateway.com";

        // 9. Minimal Student Profile Edge Case (No skills/specialties assigned)
        public static readonly Guid LiamWilsonId = Guid.Parse("30000000-0000-0000-0000-000000000009");
        public const string LiamWilsonEmail = "student.liam@academicgateway.com";

        // 10. Long Name String Edge Case (UI layout testing)
        public static readonly Guid AlexanderHohenzollernId = Guid.Parse("30000000-0000-0000-0000-000000000010");
        public const string AlexanderHohenzollernEmail = "student.alexander@academicgateway.com";
    }

    /// <summary>
    /// Deterministic accounts and profiles for industry corporate providers.
    /// </summary>
    public static class Providers
    {
        // 1. Pending Review Application
        public static readonly Guid AcmeCorpId = Guid.Parse("40000000-0000-0000-0000-000000000001");
        public const string AcmeCorpEmail = "partner@acmesolutions.internal";

        // 2. Verified / Approved Corporate Partner
        public static readonly Guid CloudSystemsId = Guid.Parse("40000000-0000-0000-0000-000000000002");
        public const string CloudSystemsEmail = "verified-partner@cloudsystems.internal";

        // 3. Rejected Application Edge Case
        public static readonly Guid CyberShieldId = Guid.Parse("40000000-0000-0000-0000-000000000003");
        public const string CyberShieldEmail = "contact@cybershield.internal";

        // 4. Resubmitted / Pending Secondary Review
        public static readonly Guid ApexGamesId = Guid.Parse("40000000-0000-0000-0000-000000000004");
        public const string ApexGamesEmail = "partner@apexgames.internal";
    }

    /// <summary>
    /// Deterministic technical support mentor accounts attached to corporate providers.
    /// </summary>
    public static class TechSupport
    {
        // Attached to Cloud Systems (Tier 3 Architect)
        public static readonly Guid AlanVanceId = Guid.Parse("50000000-0000-0000-0000-000000000001");
        public const string AlanVanceEmail = "mentor.alan@cloudsystems.internal";

        // Attached to Cloud Systems (Tier 1 DevOps)
        public static readonly Guid SarahConnorId = Guid.Parse("50000000-0000-0000-0000-000000000002");
        public const string SarahConnorEmail = "tech1@cloudsystems.internal";

        // Attached to CyberShield Dynamics (Security Operations)
        public static readonly Guid DevonMilesId = Guid.Parse("50000000-0000-0000-0000-000000000003");
        public const string DevonMilesEmail = "support@cybershield.internal";
    }
}