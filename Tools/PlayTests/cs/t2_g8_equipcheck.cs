//#HEADER
var helm = eq.Get(0);
var sel = es.currentSelectedGameObject;
return "Helm=" + (helm != null ? helm.id : "-") + " Auswahl=" + (sel != null ? sel.name : "-") + " aktiv=" + (sel != null && sel.activeInHierarchy);
