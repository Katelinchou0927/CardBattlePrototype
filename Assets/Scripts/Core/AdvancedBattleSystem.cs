using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AdvancedBattleSystem : MonoBehaviour
{
    [Header("Game Settings")]
    public int playerCount = 4;
    [Header("UI References")]
    public BattleUIManager uiManager;
    public AttackTargetUI attackTargetUI; // 攻击目标选择UI
    public SkillUIManager skillUI; // 技能选择UI

    [Header("Game State")]
    public int currentRound = 1;
    public int currentTurn = 1;
    public List<PlayerData> players = new List<PlayerData>();
    public List<int> discardPile = new List<int>();

    private PlayerData currentAttacker;
    private PlayerData currentTarget;
    private bool roundActive = false;
    private bool gameActive = false;

    // 临时变量用于存储选择结果
    private PlayerData tempSelectedTarget;

    public static System.Action<PlayerData, PlayerData, int> OnAttackExecuted;
    public static System.Action<PlayerData> OnPlayerEliminated;
    public static System.Action<int> OnRoundStart;
    public static System.Action<int> OnRoundEnd;
    public static System.Action<PlayerData> OnGameEnd;

    void Start()
    {
        Debug.Log("[AdvancedBattle] System ready, waiting for initialization");
    }

    /// <summary>
    /// 初始化战斗（由GameManager调用）
    /// </summary>
    public void InitializeBattle(int humanHP, int humanATK, int round = 1)
    {
        Debug.Log($"[AdvancedBattle] Initializing Round {round} battle with human selection: HP={humanHP}, ATK={humanATK}");
        
        currentRound = round;
        
        // 如果是第一轮，创建所有玩家
        if (currentRound == 1)
        {
            CreatePlayers(humanHP, humanATK);
            InitializeUIDisplay();
            gameActive = true;
        }
        else
        {
            // 后续轮次，更新人类玩家数值和AI选择
            UpdatePlayersForNewRound(humanHP, humanATK);
        }
        
        // 开始本轮战斗
        StartCoroutine(SingleRoundBattle());
    }

    /// <summary>
    /// 创建所有玩家（仅第一轮调用）
    /// </summary>
    void CreatePlayers(int humanHP, int humanATK)
    {
        players.Clear();
        
        // 创建人类玩家
        PlayerData humanPlayer = new PlayerData("You", 0, true);
        humanPlayer.currentHP = humanHP;
        humanPlayer.currentATK = humanATK;
        // 从数字牌中移除已使用的牌
        humanPlayer.numberCards.Remove(humanHP);
        humanPlayer.numberCards.Remove(humanATK);
        players.Add(humanPlayer);
        
        // 创建AI玩家
        for (int i = 1; i < playerCount; i++)
        {
            PlayerData aiPlayer = new PlayerData($"AI Player {i}", i, false);
            
            var aiSelection = AICardSelector.GenerateAISelection(aiPlayer.playerName, i - 1);
            aiPlayer.currentHP = aiSelection.hpCard;
            aiPlayer.currentATK = aiSelection.atkCard;
            aiPlayer.numberCards.Remove(aiSelection.hpCard);
            aiPlayer.numberCards.Remove(aiSelection.atkCard);
            
            players.Add(aiPlayer);
        }
        
        Debug.Log($"[AdvancedBattle] Created {players.Count} players");
    }

    /// <summary>
    /// 更新玩家数值（第2-5轮调用）
    /// </summary>
    void UpdatePlayersForNewRound(int humanHP, int humanATK)
    {
        Debug.Log($"[AdvancedBattle] Updating players for Round {currentRound}");
        
        // 重置所有玩家的轮次状态
        foreach (var player in players)
        {
            player.ResetForNewRound();
        }
        
        // 更新人类玩家
        PlayerData humanPlayer = players.FirstOrDefault(p => p.isHuman);
        if (humanPlayer != null)
        {
            humanPlayer.currentHP = humanHP;
            humanPlayer.currentATK = humanATK;
            humanPlayer.numberCards.Remove(humanHP);
            humanPlayer.numberCards.Remove(humanATK);
        }
        
        // AI玩家选择新的卡牌
        foreach (var player in players.Where(p => !p.isHuman))
        {
            if (player.numberCards.Count >= 2)
            {
                AISelectCardsForRound(player);
            }
            else
            {
                Debug.LogWarning($"[AdvancedBattle] {player.playerName} doesn't have enough cards for Round {currentRound}");
            }
        }
        
        // 更新UI显示
        UpdateUIForNewRound();
    }

    /// <summary>
    /// AI玩家为新轮次选择卡牌
    /// </summary>
    void AISelectCardsForRound(PlayerData aiPlayer)
    {
        if (aiPlayer.numberCards.Count < 2)
        {
            Debug.LogWarning($"[AdvancedBattle] {aiPlayer.playerName} doesn't have enough cards!");
            return;
        }
        
        List<int> available = new List<int>(aiPlayer.numberCards);
        
        // 根据不同AI策略选择
        int hp, atk;
        if (aiPlayer.playerID == 1) // 攻击型
        {
            available.Sort((a, b) => b.CompareTo(a)); // 降序
            atk = available[0]; // 最大值作为攻击
            available.RemoveAt(0);
            hp = available[Random.Range(0, Mathf.Min(3, available.Count))];
        }
        else if (aiPlayer.playerID == 2) // 防御型
        {
            available.Sort((a, b) => b.CompareTo(a)); // 降序
            hp = available[0]; // 最大值作为HP
            available.RemoveAt(0);
            atk = available[Random.Range(0, Mathf.Min(3, available.Count))];
        }
        else // 平衡型
        {
            int index1 = Random.Range(0, available.Count);
            hp = available[index1];
            available.RemoveAt(index1);
            
            int index2 = Random.Range(0, available.Count);
            atk = available[index2];
        }
        
        aiPlayer.currentHP = hp;
        aiPlayer.currentATK = atk;
        aiPlayer.numberCards.Remove(hp);
        aiPlayer.numberCards.Remove(atk);
        
        Debug.Log($"[AdvancedBattle] {aiPlayer.playerName} selected for Round {currentRound}: HP={hp}, ATK={atk}");
    }

    /// <summary>
    /// 单轮战斗流程
    /// </summary>
    IEnumerator SingleRoundBattle()
    {
        Debug.Log($"[AdvancedBattle] === ROUND {currentRound} BATTLE START ===");
        
        if (uiManager != null)
        {
            uiManager.AddBattleLog($"=== ROUND {currentRound} BATTLE START ===");
            uiManager.UpdateRoundInfo(currentRound, 0);
        }
        
        roundActive = true;
        currentTurn = 0;
        
        // 持续进行轮次直到只剩一个玩家
        while (GetAlivePlayersCount() > 1)
        {
            currentTurn++;
            yield return StartCoroutine(TurnLoop());
            yield return new WaitForSeconds(1.5f);
        }
        
        // 本轮战斗结束
        yield return StartCoroutine(RoundEnd());
    }

    IEnumerator TurnLoop()
    {
        Debug.Log($"[AdvancedBattle] Round {currentRound}, Turn {currentTurn}");
        
        if (uiManager != null)
        {
            uiManager.UpdateRoundInfo(currentRound, currentTurn);
        }
        
        // 随机选择攻击者
        currentAttacker = SelectRandomAttacker();
        if (currentAttacker == null)
        {
            Debug.LogError("[AdvancedBattle] No attacker available!");
            yield break;
        }

        Debug.Log($"[AdvancedBattle] {currentAttacker.playerName} is the attacker");
        
        if (uiManager != null)
        {
            uiManager.AddBattleLog($"Turn {currentTurn}: {currentAttacker.playerName}'s turn to attack!");
        }

        // 选择目标 - 如果是人类玩家则显示选择界面，否则AI自动选择
        if (currentAttacker.isHuman)
        {
            yield return StartCoroutine(SelectTargetByPlayerCoroutine(currentAttacker));
            currentTarget = tempSelectedTarget;
        }
        else
        {
            currentTarget = SelectTargetByAI(currentAttacker);
        }

        if (currentTarget == null)
        {
            Debug.LogError("[AdvancedBattle] No target selected!");
            yield break;
        }
        
        Debug.Log($"[AdvancedBattle] {currentAttacker.playerName} attacks {currentTarget.playerName}");
        
        if (uiManager != null)
        {
            uiManager.AddBattleLog($"{currentAttacker.playerName} attacks {currentTarget.playerName}!");
        }
        
        // 执行攻击
        yield return StartCoroutine(ExecuteAttack());
        
        // 检查目标是否死亡
        if (currentTarget.currentHP <= 0)
        {
            EliminatePlayer(currentTarget);
            currentAttacker.killsThisRound++;
            currentAttacker.totalScore++;
            
            Debug.Log($"[AdvancedBattle] {currentTarget.playerName} eliminated! {currentAttacker.playerName} scores +1");
            
            if (uiManager != null)
            {
                uiManager.AddBattleLog($"{currentTarget.playerName} is eliminated!");
                uiManager.AddBattleLog($"{currentAttacker.playerName} scores! (Total: {currentAttacker.totalScore})");
                
                if (currentAttacker.battlePlayerRef != null)
                {
                    currentAttacker.battlePlayerRef.name = $"{currentAttacker.playerName} (Score: {currentAttacker.totalScore})";
                }
                
                uiManager.UpdateAllPlayerCards();
            }
        }
    }

    /// <summary>
    /// 人类玩家选择攻击目标的协程
    /// </summary>
    IEnumerator SelectTargetByPlayerCoroutine(PlayerData attacker)
    {
        Debug.Log($"[AdvancedBattle] {attacker.playerName} (human) selecting target...");
        
        // 获取可选目标列表
        List<PlayerData> possibleTargets = players.Where(p => p.isAliveThisRound && p != attacker).ToList();
        
        if (possibleTargets.Count == 0)
        {
            Debug.LogWarning("[AdvancedBattle] No valid targets available!");
            tempSelectedTarget = null;
            yield break;
        }
        
        // 显示目标选择UI
        if (attackTargetUI != null)
        {
            yield return StartCoroutine(attackTargetUI.ShowTargetSelection(attacker, possibleTargets));
            tempSelectedTarget = attackTargetUI.GetSelectedTarget();
        }
        else
        {
            Debug.LogWarning("[AdvancedBattle] AttackTargetUI not assigned! Using random selection.");
            tempSelectedTarget = possibleTargets[Random.Range(0, possibleTargets.Count)];
        }
        
        if (tempSelectedTarget != null)
        {
            Debug.Log($"[AdvancedBattle] Player selected target: {tempSelectedTarget.playerName}");
        }
    }

    /// <summary>
    /// AI选择攻击目标
    /// </summary>
    PlayerData SelectTargetByAI(PlayerData attacker)
    {
        Debug.Log($"[AdvancedBattle] {attacker.playerName} (AI) auto-selecting target...");
        
        List<PlayerData> possibleTargets = players.Where(p => p.isAliveThisRound && p != attacker).ToList();
        if (possibleTargets.Count == 0) return null;
        
        // AI策略：优先攻击血量最低的敌人
        PlayerData target = possibleTargets.OrderBy(p => p.currentHP).First();
        
        Debug.Log($"[AdvancedBattle] AI {attacker.playerName} targets {target.playerName} (HP: {target.currentHP})");
        return target;
    }

    PlayerData SelectRandomAttacker()
    {
        List<PlayerData> alivePlayers = players.Where(p => p.isAliveThisRound).ToList();
        if (alivePlayers.Count == 0) return null;
        
        return alivePlayers[Random.Range(0, alivePlayers.Count)];
    }

    /// <summary>
    /// 修改AdvancedBattleSystem.cs中的攻击执行逻辑
    /// </summary>
    IEnumerator ExecuteAttack()
{
    // 显示攻击动画
    if (uiManager != null && currentAttacker.battlePlayerRef != null && currentTarget.battlePlayerRef != null)
    {
        uiManager.ShowAttackAnimation(currentAttacker.battlePlayerRef, currentTarget.battlePlayerRef);
        yield return new WaitForSeconds(1f);
    }
    
    // ======= 新增：攻击方技能选择阶段 =======
    bool attackerUsedSkill = false;
    string attackerSkill = "";
    
    if (currentAttacker.isHuman && HasAvailableSkills(currentAttacker))
    {
        Debug.Log($"[AdvancedBattle] {currentAttacker.playerName} can use attack skills");
        
        // 显示技能选择面板
        if (skillUI != null)
        {
            yield return StartCoroutine(skillUI.ShowSkillChoice($"Your turn to attack {currentTarget.playerName}!", currentAttacker));
            attackerSkill = skillUI.GetSelectedSkill();
            attackerUsedSkill = !string.IsNullOrEmpty(attackerSkill) && attackerSkill != "SKIP";
        }
    }
    
    // 处理攻击方技能效果
    if (attackerUsedSkill)
    {
        yield return StartCoroutine(ProcessAttackerSkill(attackerSkill, currentAttacker, currentTarget));
    }
    
    // ======= 新增：防御方技能选择阶段 =======
    bool defenderUsedSkill = false;
    string defenderSkill = "";
    
    // 检查是否需要防御技能（受到致命伤害或特定条件）
    bool needsDefense = ShouldOfferDefenseSkill(currentAttacker, currentTarget, attackerUsedSkill);
    
    if (needsDefense && currentTarget.isHuman && HasAvailableSkills(currentTarget))
    {
        Debug.Log($"[AdvancedBattle] {currentTarget.playerName} can use defense skills");
        
        if (skillUI != null)
        {
            string defensePrompt = attackerUsedSkill ? 
                $"Enemy used skill! Defend yourself!" : 
                $"You're under attack from {currentAttacker.playerName}!";
                
            yield return StartCoroutine(skillUI.ShowSkillChoice(defensePrompt, currentTarget));
            defenderSkill = skillUI.GetSelectedSkill();
            defenderUsedSkill = !string.IsNullOrEmpty(defenderSkill) && defenderSkill != "SKIP";
        }
    }
    
    // 处理防御方技能效果
    if (defenderUsedSkill)
    {
        yield return StartCoroutine(ProcessDefenderSkill(defenderSkill, currentTarget, currentAttacker));
    }
    
    // ======= 计算最终伤害 =======
    int finalDamage = CalculateFinalDamage(currentAttacker, currentTarget, attackerSkill, defenderSkill);
    
    if (finalDamage > 0)
    {
        // 应用伤害
        currentTarget.currentHP -= finalDamage;
        currentTarget.currentHP = Mathf.Max(0, currentTarget.currentHP);
        
        // 同步更新BattlePlayer
        if (currentTarget.battlePlayerRef != null)
        {
            currentTarget.battlePlayerRef.TakeDamage(finalDamage);
        }
        
        Debug.Log($"[AdvancedBattle] {currentTarget.playerName} takes {finalDamage} damage, HP: {currentTarget.currentHP}");
        
        // 显示伤害动画
        if (uiManager != null && currentTarget.battlePlayerRef != null)
        {
            uiManager.ShowDamageAnimation(currentTarget.battlePlayerRef, finalDamage);
            yield return new WaitForSeconds(0.5f);
            uiManager.UpdateAllPlayerCards();
        }
    }
    else
    {
        Debug.Log($"[AdvancedBattle] Attack was blocked or had no effect!");
        if (uiManager != null)
        {
            uiManager.AddBattleLog("Attack was blocked!");
        }
    }
    
    OnAttackExecuted?.Invoke(currentAttacker, currentTarget, finalDamage);
}
    

    /// <summary>
    /// 检查玩家是否有可用技能
    /// </summary>
    bool HasAvailableSkills(PlayerData player)
    {
        if (!player.isHuman) return false; // 只有人类玩家有技能UI
    
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return false;
    
        var characterInfo = gameManager.GetPlayerCharacterInfo();
        return characterInfo.hasSkills;
    }
    
    /// <summary>
/// 判断是否应该提供防御技能选择
/// </summary>
bool ShouldOfferDefenseSkill(PlayerData attacker, PlayerData target, bool attackerUsedSkill)
{
    // 如果攻击方使用了技能，总是给防御方机会
    if (attackerUsedSkill) return true;
    
    // 如果攻击会造成致命伤害，给防御方机会
    if (attacker.currentATK >= target.currentHP) return true;
    
    // 其他情况下也可以使用防御技能（比如Q技能回血）
    return true;
}

/// <summary>
/// 处理攻击方技能效果
/// </summary>
IEnumerator ProcessAttackerSkill(string skillType, PlayerData attacker, PlayerData target)
{
    Debug.Log($"[AdvancedBattle] Processing attacker skill: {skillType}");
    
    GameManager gameManager = GameManager.Instance;
    var characterInfo = gameManager.GetPlayerCharacterInfo();
    
    if (uiManager != null)
    {
        uiManager.AddBattleLog($"{attacker.playerName} uses skill {skillType}!");
    }
    
    switch (characterInfo.characterIndex)
    {
        case 1: // Field Commander - 群体技能
            yield return StartCoroutine(ProcessFieldCommanderSkill(skillType, attacker));
            break;
        case 2: // Shadow Duelist - 个人技能
            yield return StartCoroutine(ProcessShadowDuelistSkill(skillType, attacker, target));
            break;
    }
    
    yield return new WaitForSeconds(1f); // 技能效果展示时间
}

/// <summary>
/// 处理防御方技能效果
/// </summary>
IEnumerator ProcessDefenderSkill(string skillType, PlayerData defender, PlayerData attacker)
{
    Debug.Log($"[AdvancedBattle] Processing defender skill: {skillType}");
    
    GameManager gameManager = GameManager.Instance;
    var characterInfo = gameManager.GetPlayerCharacterInfo();
    
    if (uiManager != null)
    {
        uiManager.AddBattleLog($"{defender.playerName} uses skill {skillType}!");
    }
    
    switch (characterInfo.characterIndex)
    {
        case 1: // Field Commander - 群体技能
            yield return StartCoroutine(ProcessFieldCommanderSkill(skillType, defender));
            break;
        case 2: // Shadow Duelist - 个人技能
            yield return StartCoroutine(ProcessShadowDuelistSkill(skillType, defender, attacker));
            break;
    }
    
    yield return new WaitForSeconds(1f); // 技能效果展示时间
}

/// <summary>
/// Process Field Commander (Character 1) skill effects - English version
/// </summary>
IEnumerator ProcessFieldCommanderSkill(string skillType, PlayerData user)
{
    Debug.Log($"[AdvancedBattle] Processing Field Commander skill: {skillType}");
    
    switch (skillType)
    {
        case "J": // All players lose 2 HP
            if (uiManager != null)
            {
                uiManager.AddBattleLog($"{user.playerName} uses Global Damage! All players lose 2 HP!");
            }
            
            foreach (var player in players)
            {
                if (player.isAliveThisRound)
                {
                    int damage = 2;
                    player.currentHP -= damage;
                    player.currentHP = Mathf.Max(0, player.currentHP);
                    
                    // Update UI
                    if (player.battlePlayerRef != null)
                    {
                        player.battlePlayerRef.UpdateHP(player.currentHP);
                    }
                    
                    Debug.Log($"[AdvancedBattle] {player.playerName} loses 2 HP, current HP: {player.currentHP}");
                    
                    // Check if eliminated
                    if (player.currentHP <= 0 && player != user)
                    {
                        EliminatePlayer(player);
                        if (uiManager != null)
                        {
                            uiManager.AddBattleLog($"{player.playerName} eliminated by global damage!");
                        }
                    }
                }
            }
            break;
            
        case "Q": // All players gain 1 HP
            if (uiManager != null)
            {
                uiManager.AddBattleLog($"{user.playerName} uses Global Heal! All players gain 1 HP!");
            }
            
            foreach (var player in players)
            {
                if (player.isAliveThisRound)
                {
                    player.currentHP += 1;
                    
                    // Update UI
                    if (player.battlePlayerRef != null)
                    {
                        player.battlePlayerRef.UpdateHP(player.currentHP);
                    }
                    
                    Debug.Log($"[AdvancedBattle] {player.playerName} gains 1 HP, current HP: {player.currentHP}");
                }
            }
            break;
            
        case "K": // All players lose 1 ATK
            if (uiManager != null)
            {
                uiManager.AddBattleLog($"{user.playerName} uses Global Weaken! All players lose 1 ATK!");
            }
            
            foreach (var player in players)
            {
                if (player.isAliveThisRound)
                {
                    player.currentATK -= 1;
                    player.currentATK = Mathf.Max(0, player.currentATK);
                    
                    // Update UI
                    if (player.battlePlayerRef != null)
                    {
                        player.battlePlayerRef.UpdateAttack(player.currentATK);
                    }
                    
                    Debug.Log($"[AdvancedBattle] {player.playerName} loses 1 ATK, current ATK: {player.currentATK}");
                }
            }
            break;
    }
    
    // Update all UI displays
    if (uiManager != null)
    {
        uiManager.UpdateAllPlayerCards();
    }
    
    yield return new WaitForSeconds(1.5f);
}

/// <summary>
/// Process Shadow Duelist (Character 2) skill effects - English version  
/// </summary>
IEnumerator ProcessShadowDuelistSkill(string skillType, PlayerData user, PlayerData target = null)
{
    Debug.Log($"[AdvancedBattle] Processing Shadow Duelist skill: {skillType}");
    
    switch (skillType)
    {
        case "J": // Exchange HP or ATK with target
            if (target != null && target != user && target.isAliveThisRound)
            {
                if (uiManager != null)
                {
                    uiManager.AddBattleLog($"{user.playerName} uses Stat Exchange with {target.playerName}!");
                }
                
                // Simplified version: Exchange HP (you can add UI choice later)
                int tempHP = user.currentHP;
                user.currentHP = target.currentHP;
                target.currentHP = tempHP;
                
                // Update UI
                if (user.battlePlayerRef != null)
                {
                    user.battlePlayerRef.UpdateHP(user.currentHP);
                }
                if (target.battlePlayerRef != null)
                {
                    target.battlePlayerRef.UpdateHP(target.currentHP);
                }
                
                if (uiManager != null)
                {
                    uiManager.AddBattleLog($"HP exchanged! {user.playerName}: {user.currentHP} HP, {target.playerName}: {target.currentHP} HP");
                }
                
                Debug.Log($"[AdvancedBattle] HP exchanged! {user.playerName}: {user.currentHP}, {target.playerName}: {target.currentHP}");
            }
            else
            {
                Debug.LogWarning("[AdvancedBattle] Invalid target for stat exchange");
                if (uiManager != null)
                {
                    uiManager.AddBattleLog($"{user.playerName} tried to use Stat Exchange but no valid target!");
                }
            }
            break;
            
        case "Q": // Gain 5 HP for self
            if (uiManager != null)
            {
                uiManager.AddBattleLog($"{user.playerName} uses Self Heal! Gains 5 HP!");
            }
            
            user.currentHP += 5;
            
            // Update UI
            if (user.battlePlayerRef != null)
            {
                user.battlePlayerRef.UpdateHP(user.currentHP);
            }
            
            Debug.Log($"[AdvancedBattle] {user.playerName} gains 5 HP, current HP: {user.currentHP}");
            break;
            
        case "K": // Survive fatal attack with 1 HP
            if (uiManager != null)
            {
                uiManager.AddBattleLog($"{user.playerName} uses Last Stand! Survives with 1 HP!");
            }
            
            // This skill should trigger when about to die, ensure at least 1 HP remains
            if (user.currentHP <= 0)
            {
                user.currentHP = 1;
                
                // Update UI
                if (user.battlePlayerRef != null)
                {
                    user.battlePlayerRef.UpdateHP(user.currentHP);
                    user.battlePlayerRef.isEliminated = false; // Cancel elimination
                }
                
                Debug.Log($"[AdvancedBattle] {user.playerName} survives with 1 HP using Last Stand!");
            }
            else
            {
                // Can also be used preemptively to prepare for next attack
                if (uiManager != null)
                {
                    uiManager.AddBattleLog($"{user.playerName} prepares Last Stand defense!");
                }
            }
            break;
    }
    
    // Update all UI displays
    if (uiManager != null)
    {
        uiManager.UpdateAllPlayerCards();
    }
    
    yield return new WaitForSeconds(1.5f);
}

/// <summary>
/// Calculate final damage with English logging - Updated for one-time skills
/// </summary>
int CalculateFinalDamage(PlayerData attacker, PlayerData target, string attackerSkill, string defenderSkill)
{
    GameManager gameManager = GameManager.Instance;
    var characterInfo = gameManager.GetPlayerCharacterInfo();
    
    int baseDamage = attacker.currentATK;
    int finalDamage = baseDamage;
    bool attackBlocked = false;
    
    // Process defender skills first (defensive priority)
    if (!string.IsNullOrEmpty(defenderSkill) && defenderSkill != "SKIP")
    {
        if (characterInfo.characterIndex == 2 && defenderSkill == "K") // Shadow Duelist K skill
        {
            // Last Stand - survive with 1 HP
            if (baseDamage >= target.currentHP)
            {
                finalDamage = 0; // Block this attack
                attackBlocked = true;
                
                // Ensure target survives with 1 HP
                target.currentHP = 1;
                if (target.battlePlayerRef != null)
                {
                    target.battlePlayerRef.UpdateHP(1);
                }
                
                if (uiManager != null)
                {
                    uiManager.AddBattleLog($"{target.playerName} uses Last Stand! Attack nullified!");
                }
            }
        }
    }
    
    // Process attacker skills (if attack wasn't blocked)
    if (!attackBlocked && !string.IsNullOrEmpty(attackerSkill) && attackerSkill != "SKIP")
    {
        if (characterInfo.characterIndex == 2 && attackerSkill == "J") // Shadow Duelist J skill
        {
            // Note: J skill for Shadow Duelist is stat exchange, not instant kill
            // The actual exchange is handled in ProcessShadowDuelistSkill
            // Here we just use normal damage
        }
        else if (characterInfo.characterIndex == 1) // Field Commander skills
        {
            // Field Commander skills are area effects, handled separately
            // Normal attack still proceeds
        }
    }
    
    // Final damage logging
    if (finalDamage > 0 && uiManager != null)
    {
        uiManager.AddBattleLog($"{target.playerName} takes {finalDamage} damage!");
    }
    else if (attackBlocked && uiManager != null)
    {
        uiManager.AddBattleLog($"Attack completely blocked by {target.playerName}!");
    }
    
    return finalDamage;
}

    /// <summary>
    /// 轮次结束处理
    /// </summary>
    IEnumerator RoundEnd()
    {
        Debug.Log($"[AdvancedBattle] === ROUND {currentRound} END ===");
        OnRoundEnd?.Invoke(currentRound);
        
        if (uiManager != null)
        {
            uiManager.AddBattleLog($"=== ROUND {currentRound} END ===");
            
            // 显示本轮得分
            foreach (var player in players)
            {
                if (player.killsThisRound > 0)
                {
                    uiManager.AddBattleLog($"{player.playerName}: +{player.killsThisRound} points this round (Total: {player.totalScore})");
                }
            }
        }
        
        ProcessQSkill();
        roundActive = false;
        
        yield return new WaitForSeconds(2f);
        
        // 检查是否继续下一轮或结束游戏
        if (ShouldContinueGame())
        {
            // 通知GameManager准备下一轮
            GameManager.Instance?.OnRoundComplete();
        }
        else
        {
            // 游戏结束
            EndGame();
        }
    }

    /// <summary>
    /// 检查是否应该继续游戏
    /// </summary>
    bool ShouldContinueGame()
    {
        // 检查是否已完成5轮
        if (currentRound >= 5)
        {
            Debug.Log("[AdvancedBattle] 5 rounds completed, ending game");
            return false;
        }
        
        // 检查玩家是否还有足够的卡牌
        bool hasEnoughCards = players.Any(p => p.numberCards.Count >= 2);
        if (!hasEnoughCards)
        {
            Debug.Log("[AdvancedBattle] No players have enough cards, ending game");
            return false;
        }
        
        return true;
    }

    /// <summary>
    /// 初始化UI显示
    /// </summary>
    void InitializeUIDisplay()
    {
        if (uiManager == null)
        {
            Debug.LogError("[AdvancedBattle] UI Manager not assigned!");
            return;
        }
        
        List<BattlePlayer> battlePlayers = new List<BattlePlayer>();
        
        foreach (var player in players)
        {
            BattlePlayer bp = new BattlePlayer(
                $"{player.playerName} (Score: {player.totalScore})",
                player.currentHP,
                player.currentATK
            );
            
            player.battlePlayerRef = bp;
            battlePlayers.Add(bp);
            
            Debug.Log($"[AdvancedBattle] Created BattlePlayer for {player.playerName}: HP={bp.currentHP}, ATK={bp.attack}");
        }
        
        uiManager.InitializePlayers(battlePlayers);
        uiManager.ShowBattleUI(); // 确保战斗UI显示
        
        Debug.Log("[AdvancedBattle] UI initialized with all players, battle UI shown");
    }

    /// <summary>
    /// 为新轮次更新UI
    /// </summary>
    void UpdateUIForNewRound()
    {
        if (uiManager == null) return;
        
        // 确保战斗UI显示（第2-5轮可能需要重新显示）
        uiManager.ShowBattleUI();
        
        foreach (var player in players)
        {
            if (player.battlePlayerRef != null)
            {
                player.battlePlayerRef.UpdateHP(player.currentHP);
                player.battlePlayerRef.UpdateAttack(player.currentATK);
                player.battlePlayerRef.name = $"{player.playerName} (Score: {player.totalScore})";
                player.battlePlayerRef.isEliminated = false; // 重置消除状态
            }
        }
        
        uiManager.UpdateAllPlayerCards();
        
        Debug.Log($"[AdvancedBattle] UI updated for Round {currentRound}, battle UI shown");
    }

    void EliminatePlayer(PlayerData player)
    {
        player.isAliveThisRound = false;
        
        if (player.battlePlayerRef != null)
        {
            player.battlePlayerRef.isEliminated = true;
        }
        
        OnPlayerEliminated?.Invoke(player);
        
        if (player.currentHP > 0) discardPile.Add(player.currentHP);
        if (player.currentATK > 0) discardPile.Add(player.currentATK);
    }

    int GetAlivePlayersCount()
    {
        return players.Count(p => p.isAliveThisRound);
    }

    void ProcessQSkill()
    {
        var deadPlayers = players.Where(p => !p.isAliveThisRound).ToList();
        
        foreach (var player in deadPlayers)
        {
            if (player.hasQ && player.numberCards.Count == 0)
            {
                UseQSkill(player);
            }
        }
    }

    void UseQSkill(PlayerData player)
    {
        if (!player.hasQ || discardPile.Count < 2) return;
        
        player.hasQ = false;
        
        for (int i = 0; i < 2 && discardPile.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, discardPile.Count);
            int card = discardPile[randomIndex];
            discardPile.RemoveAt(randomIndex);
            player.numberCards.Add(card);
        }
        
        Debug.Log($"[AdvancedBattle] {player.playerName} uses Q skill, draws {player.numberCards.Count} cards");
        
        if (uiManager != null)
        {
            uiManager.AddBattleLog($"{player.playerName} uses REDRAW (Q), gets 2 cards from discard pile");
        }
    }

    /// <summary>
    /// Updated end game with English text
    /// </summary>
    void EndGame()
    {
        gameActive = false;
    
        PlayerData winner = players.OrderByDescending(p => p.totalScore).First();
    
        Debug.Log($"[AdvancedBattle] GAME END! Winner: {winner.playerName} with {winner.totalScore} points");
    
        if (uiManager != null)
        {
            uiManager.AddBattleLog($"=== GAME COMPLETED ===");
            uiManager.AddBattleLog($"WINNER: {winner.playerName} with {winner.totalScore} points!");
        
            // Show final scores in English
            uiManager.AddBattleLog("Final Standings:");
            foreach (var player in players.OrderByDescending(p => p.totalScore))
            {
                uiManager.AddBattleLog($"  {player.playerName}: {player.totalScore} points");
            }
        
            if (winner.battlePlayerRef != null)
            {
                uiManager.ShowBattleEnd(winner.battlePlayerRef);
            }
        }
    
        // Reset skills for next game
        SkillUIManager.ResetAllSkills();
    
        OnGameEnd?.Invoke(winner);
    }
}