//#HEADER
var chest = chestById("room_test_truhe_1");
chest.Open();
rb.position = (UnityEngine.Vector2)chest.transform.position + new UnityEngine.Vector2(0.9f, -0.4f);
p.transform.position = rb.position;
return "Truhe offen fuer Screenshot";
