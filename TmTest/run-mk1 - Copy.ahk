; Enter -> right arrow -> Enter -> down arrow -> Enter -> up arrow -> up arrow -> Enter -> right arrow -> down arrow 
; -> Enter -> Enter -> right arrow -> down arrow -> Enter
RunLevel(level)
{
playkeydelay:=50
SetKeyDelay, 50, %playkeydelay%
downArrowsCount:=1
rightArrowsCount:=6
if (level == 2) {
	downArrowsCount:=2
	rightArrowsCount:=3
}
if (level == 3) {
	downArrowsCount:=3
	rightArrowsCount:=1
}

MouseMove 0, 0, 0
BlockInput, MouseMove   ;block mouse input
Send, {Enter}   ;select profile
Send, {Right}   ;select "continue offline" (profile not connected to online servers)
Send, {Enter}   ;click "continue offline"
Sleep, 500      ;loading menu takes longer than other actions
Send, {Down}    ;select "play solo"
Send, {Enter}   ;click "play solo"
Send, {Up}      ;select "Back"
Send, {Up}      ;select "Browse"
Send, {Enter}   ;click "Browse"
Send, {Right}   ;select "directory up"
Send, {Down}    ;select "Challenges\" directory
Send, {Enter}   ;click "Challenges\" directory
Send, {Enter}   ;click "My Challenges\" directory
Send, {Right}   ;select "directory up"
Loop, %downArrowsCount% {
	Send, {Down}
}
;Send, {Down 3}    ;select track - 1 = easy, 2 = medium, 3 = hard
Send, {Enter}   ;click on first track to select it as level to play
Loop, %rightArrowsCount% {
	Send, {Right}
}
;Send, {Right 1} ; select and play map - 6 = easy, 3 = medium, 1 = hard
Send, {Enter}   ;click "Play"
Sleep, 700
;user should see screen on which he is forced to press any key to start race
Send, {Enter}
BlockInput, MouseMoveOff    ;unblock mouse input
}