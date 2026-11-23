# Gilden-Test (Meilenstein 7c): Beitreten (Bonus, Waffe), 3 Quests der Kriegergilde (Besiegen, Bringen) bis Rang 2,
# Wechsel zur Magiergilde (zu wenig / genug Gold), Zauber-Quest, Speichern und Laden (Version 4),
# Wald: Gegner vorhanden, Wolf und Spinne treffen, sterben, lassen Beute fallen; Rueckweg in die Hoehle.
# Start ueber run_all.ps1 (schuetzt den Spielstand).
. "$PSScriptRoot\lib.ps1"
RequireSession
$f0 = $global:RebornFails

'--- Gilden-Test ---'
StartPlay -Scene 'Base'
'  ' + (Eval 't2_setup.cs')

function Talk() {
    [void](Eval 'b_goto.cs' @{ X = '0'; Y = '1.6' }); Wait 300
    [void](Eval 'b_key.cs' @{ KEY = 'E' }); Wait 200; [void](Eval 'b_keyup.cs'); Wait 300
}
function Go([string]$door, [string]$snippet = 'b_door_go.cs') {
    [void](Eval $snippet @{ NAME = $door }); Wait 2500; WaitReady
    '  ' + (Eval 't2_setup.cs')
}

'G1) Kriegergilde beitreten'
Go 'Tuer_Gilde_Krieger'
Talk
Check 'G1 Gilden-Fenster offen' (Eval 'b_trade.cs') ' offen=True Seite=Gilde Zeit=0 '
Check 'G1 vorher keine Gilde' (Eval 'g_guild.cs' @{ ACT = 'state' }) '\| Gilde=None Rang=0 .* Staerke=1 '
Check 'G1 Beitritt: Staerke +2, Gildenschwert' (Eval 'g_guild.cs' @{ ACT = 'join' }) '^Meldung=Willkommen in der Kriegergilde! \| Gilde=Krieger Rang=1 Quest=0 aktiv=False Fortschritt=0/5 Staerke=3 .* Waffe=gildenschwert:1/\+0 HUD=$'
Shot 'm7c_gilde'

'G2) Quest 1: Schleime besiegen'
Check 'G2 angenommen, HUD zeigt die Quest' (Eval 'g_guild.cs' @{ ACT = 'accept' }) 'Quest=0 aktiv=True Fortschritt=0/5 .* HUD=Schleime besiegen 0/5$'
Check 'G2 Woelfe zaehlen nicht' (Eval 'g_guild.cs' @{ ACT = 'kill:wolf:2:0' }) 'Fortschritt=0/5 '
Check 'G2 Abgeben zu frueh geht nicht' (Eval 'g_guild.cs' @{ ACT = 'turnin' }) '^Meldung=Noch nicht fertig \(0/5\) '
Check 'G2 5 Schleime' (Eval 'g_guild.cs' @{ ACT = 'kill:schleim:5:0' }) 'Fortschritt=5/5 '
Check 'G2 Abgeben: +50 Gold' (Eval 'g_guild.cs' @{ ACT = 'turnin' }) '^Meldung=Quest erledigt: \+50 Gold, \+60 EP \| Gilde=Krieger Rang=1 Quest=1 aktiv=False .* Gold=50 '

'G3) Quest 2: Woelfe, Quest 3: Wolfsfelle -> Rang 2'
[void](Eval 'g_guild.cs' @{ ACT = 'accept' })
Check 'G3 3 Woelfe, +80 Gold' (Eval 'g_guild.cs' @{ ACT = 'kill:wolf:3:0' }) 'Quest=1 aktiv=True Fortschritt=3/3 '
Check 'G3 Quest 2 abgegeben' (Eval 'g_guild.cs' @{ ACT = 'turnin' }) 'Quest=2 aktiv=False .* Gold=130 '
[void](Eval 'g_guild.cs' @{ ACT = 'accept' })
Check 'G3 4 Wolfsfelle im Inventar' (Eval 'g_guild.cs' @{ ACT = 'give:wolfsfell:4' }) 'Quest=2 aktiv=True Fortschritt=4/4 '
Check 'G3 Abgeben: Rang Geselle, Staerke +4, Schwert +2' (Eval 'g_guild.cs' @{ ACT = 'turnin' }) '^Meldung=Quest erledigt: \+120 Gold, \+150 EP . Neuer Rang: Geselle! \| Gilde=Krieger Rang=2 Quest=3 aktiv=False Fortschritt=0/0 Staerke=5 .* Gold=250 .* Waffe=gildenschwert:1/\+2 HUD=$'
Check 'G3 Felle abgegeben' (Eval 'b_shop.cs' @{ ACT = 'sell:wolfsfell' }) 'Anzahl=0$'
Check 'G3 keine weitere Quest' (Eval 'g_guild.cs' @{ ACT = 'accept' }) '^Meldung=Alle Quests erledigt '
[void](Eval 'b_close.cs')

'G4) Wechsel zur Magiergilde'
Go 'Tuer_Ausgang'
Go 'Tuer_Gilde_Magier'
Talk
Check 'G4 Magier-Fenster' (Eval 'b_trade.cs') ' offen=True Seite=Gilde '
[void](Eval 'b_shop.cs' @{ ACT = 'setgold:50' })
Check 'G4 zu wenig Gold' (Eval 'g_guild.cs' @{ ACT = 'join' }) '^Meldung=Wechsel kostet 200 Gold \| Gilde=Krieger Rang=2 '
[void](Eval 'b_shop.cs' @{ ACT = 'setgold:300' })
Check 'G4 Wechsel: -200 Gold, Intelligenz +2, Stab' (Eval 'g_guild.cs' @{ ACT = 'join' }) '^Meldung=Willkommen in der Magiergilde! \| Gilde=Magier Rang=1 Quest=0 aktiv=False Fortschritt=0/5 Staerke=1 Intelligenz=3 Gold=100 .* Waffe=gildenstab:1/\+0 '

'G5) Zauber-Quest'
[void](Eval 'g_guild.cs' @{ ACT = 'accept' })
Check 'G5 ohne Zauber zaehlt nicht' (Eval 'g_guild.cs' @{ ACT = 'kill:schleim:3:0' }) 'Fortschritt=0/5 '
Check 'G5 mit Zaubern' (Eval 'g_guild.cs' @{ ACT = 'kill:spinne:5:1' }) 'Fortschritt=5/5 .* HUD=Gegner mit Zaubern besiegen 5/5$'

'G6) Speichern und Laden'
Check 'G6 Datei Version 4 mit Gilde' (Eval 'g_save.cs') '^Version=5 Gilde=Magier Rang=1 Quest=0 aktiv=True Fortschritt=5$'
Check 'G6 Laden stellt die Gilde her' (Eval 'g_reload.cs') '^geladen=True Gilde=Magier Rang=1 Quest=0 aktiv=True Fortschritt=5$'
[void](Eval 'b_close.cs')

'G7) Wald'
Go 'Tuer_Ausgang'
Go 'Tuer_Hoehle' 'b_portal_go.cs'
[void](Eval 'b_portal_go.cs' @{ NAME = 'Portal_Wald' }); Wait 2500; WaitReady
Check 'G7 im Wald angekommen' (Eval 'b_state.cs') '^Szene=Forest .* reist=False Ankunft=Ankunft$'
[void](Eval 'lk_zoom.cs' @{ SIZE = '6'; X = '-6'; Y = '-0.6' }); Wait 600
Shot 'm7c_wald'
'  ' + (Eval 't2_setup.cs')
Check 'G7 Gegner, Kampfgebiet, Boden' (Eval 'f_census.cs') '^Szene=Forest Woelfe=4 Spinnen=3 Schleime=2 friedlich=False Boden=[1-9]\d*$'
foreach ($a in @(@('wolf', '(8|16)'), @('spinne', '(12|24)'))) {
    '  ' + (Eval 'f_wolf_go.cs' @{ ART = $a[0] }); Wait 1500
    Check "G7 $($a[0]) greift an, stirbt, Beute" (Eval 'f_wolf_check.cs' @{ ART = $a[0] }) "^Schaden=$($a[1]) Angriff=1 besiegt=True Beute=[1-9]\d* EP\+=True$"
    Wait 600
}
Shot 'm7c_kampf'
[void](Eval 'b_portal_go.cs' @{ NAME = 'Portal_Hoehle' }); Wait 2500; WaitReady
Check 'G7 Rueckweg in die Hoehle' (Eval 'b_state.cs') '^Szene=Cave .* reist=False Ankunft=Wald$'

StopPlay
TestResult 'Gilden-Test' $f0
