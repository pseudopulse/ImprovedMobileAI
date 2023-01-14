using System;

namespace ImprovedMobileAI.Tweaks {
    public enum TargetType {
        BossesFirst = 0,
        HighestHealth = 1,
        LowestHealth = 2,
        Closest = 3,
        Furthest = 4,
        None = 5
    }
    public class Drivers {
        private static float ChaseRange => ImprovedMobileAI.config.Bind<float>("AI Drivers:", "Chase Range", 60f, "The range at which a TR-58 Carbonizer Turret will attempt to chase an enemy. Vanilla is 25.").Value;
        private static float StrafeRange => ImprovedMobileAI.config.Bind<float>("AI Drivers:", "Strafe Distance", 5f, "The minimum distance at which a TR-58 Carbonizer Turret will attempt to strafe an enemy. Vanilla is 15.").Value;
        private static bool SprintOnReturn => ImprovedMobileAI.config.Bind<bool>("AI Drivers:", "Sprint on Return", true, "Should the TR-58 Carbonizer Turret sprint when returning to it's owner? Vanilla is false.").Value;
        private static TargetType TargetPriority => ImprovedMobileAI.config.Bind<TargetType>("AI Drivers:", "Target Priority", TargetType.HighestHealth, "The target priority that a TR-58 Carbonizer Turret will use.").Value;

        public static void Setup() {
            GameObject master = Utils.Paths.GameObject.EngiWalkerTurretMaster.Load<GameObject>();

            AISkillDriver[] drivers = master.GetComponents<AISkillDriver>();
            AISkillDriver strafe = drivers.First(x => x.customName == "StrafeAndFireAtEnemy");
            AISkillDriver rest = drivers.First(x => x.customName == "Rest");
            AISkillDriver chase = master.AddComponent<AISkillDriver>();
            chase.customName = "ChaseTarget";
            chase.moveTargetType = AISkillDriver.TargetType.CurrentEnemy;
            chase.movementType = AISkillDriver.MovementType.ChaseMoveTarget;
            chase.maxDistance = ChaseRange;
            chase.minDistance = 25f;
            chase.aimType = AISkillDriver.AimType.AtCurrentEnemy;
            chase.skillSlot = SkillSlot.None;
            chase.shouldSprint = SprintOnReturn;
            chase.nextHighPriorityOverride = strafe;
            AISkillDriver returnToLeader = drivers.First(x => x.customName == "ReturnToLeader" && x.shouldSprint == false);
            AISkillDriver returnToLeaderButCringe = drivers.First(x => x.customName == "ReturnToLeader" && x.shouldSprint);

            strafe.minDistance = StrafeRange;
            returnToLeader.shouldSprint = SprintOnReturn;
            returnToLeader.nextHighPriorityOverride = chase;
            rest.nextHighPriorityOverride = chase;
            returnToLeaderButCringe.nextHighPriorityOverride = chase;

            _ = TargetPriority; // run the getter so that it gens the config even if a run isnt started

            On.RoR2.CharacterAI.BaseAI.FindEnemyHurtBox += OverrideTargeting;
        }

        private static HurtBox OverrideTargeting(On.RoR2.CharacterAI.BaseAI.orig_FindEnemyHurtBox orig, BaseAI self, float dist, bool fullVis, bool losFilt) {
            if (self.body && self.body.bodyIndex == BodyCatalog.FindBodyIndex("EngiWalkerTurretBody")) {
                self.enemySearch.viewer = self.body;
                self.enemySearch.teamMaskFilter = TeamMask.allButNeutral;
                self.enemySearch.teamMaskFilter.RemoveTeam(self.master.teamIndex);
                self.enemySearch.sortMode = BullseyeSearch.SortMode.Distance;
                self.enemySearch.minDistanceFilter = 0f;
                self.enemySearch.maxDistanceFilter = dist;
                self.enemySearch.searchOrigin = self.bodyInputBank.aimOrigin;
                self.enemySearch.searchDirection = self.bodyInputBank.aimDirection;
                self.enemySearch.maxAngleFilter = (fullVis ? 180f : 90f);
                self.enemySearch.filterByLoS = losFilt;
                self.enemySearch.RefreshCandidates();

                List<HurtBox> targets;
                switch (TargetPriority) {
                    case TargetType.BossesFirst:
                        targets = self.enemySearch.GetResults().Where(x => x.healthComponent && x.healthComponent.body.isChampion).ToList();
                        if (targets.Count <= 0) {
                            targets = self.enemySearch.GetResults().ToList();
                        }
                        break;
                    case TargetType.HighestHealth:
                        targets = self.enemySearch.GetResults().Where(x => x.healthComponent).OrderByDescending(x => x.healthComponent.fullHealth).ToList();
                        targets = targets.Where(x => Vector3.Distance(x.transform.position, self.transform.position) < ChaseRange).ToList();
                        targets.RemoveAll(x => IsFlying(x) && Vector3.Distance(x.transform.position, self.body.corePosition) > 25);
                        break;
                    case TargetType.LowestHealth:
                        targets = self.enemySearch.GetResults().Where(x => x.healthComponent).OrderBy(x => x.healthComponent.fullHealth).ToList();
                        targets = targets.Where(x => Vector3.Distance(x.transform.position, self.transform.position) < ChaseRange).ToList();
                        targets.RemoveAll(x => IsFlying(x) && Vector3.Distance(x.transform.position, self.body.corePosition) > 25);
                        break;
                    case TargetType.Closest:
                        targets = self.enemySearch.GetResults().OrderBy(x => Vector3.Distance(x.transform.position, self.body.corePosition)).ToList();
                        targets = targets.Where(x => Vector3.Distance(x.transform.position, self.transform.position) < ChaseRange).ToList();
                        targets.RemoveAll(x => IsFlying(x) && Vector3.Distance(x.transform.position, self.body.corePosition) > 25);
                        break;
                    case TargetType.Furthest:
                        targets = self.enemySearch.GetResults().OrderByDescending(x => Vector3.Distance(x.transform.position, self.body.corePosition)).ToList();
                        targets = targets.Where(x => Vector3.Distance(x.transform.position, self.transform.position) < ChaseRange).ToList();
                        targets.RemoveAll(x => IsFlying(x) && Vector3.Distance(x.transform.position, self.body.corePosition) > 25);
                        break;
                    case TargetType.None:
                        return orig(self, dist, fullVis, losFilt);
                    default:
                        return orig(self, dist, fullVis, losFilt);
                }
                
                return targets.FirstOrDefault();
                
            }
            else {
                return orig(self, dist, fullVis, losFilt);
            }
        }

        private static bool IsFlying(HurtBox box) {
            if (box.healthComponent && box.healthComponent.body.GetComponent<VectorPID>()) {
                return true;
            }
            else {
                return false;
            }
        }
    }
}