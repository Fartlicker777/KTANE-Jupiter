using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using KModkit;

public class Jupiter : MonoBehaviour {

   public KMBombInfo Bomb;
   public KMAudio Audio;

   public KMSelectable HideButton;
   public KMSelectable Jupter;
   public GameObject Planet;
   public GameObject Background;
   public GameObject StatusLightPosition;

   int CurrentNumber;
   int RotationAmount;
   int Visits;

   string PreviousConnector = "";
   string PreviousNode = "";
   string PreviousOrbital = "";
   string PreviousStandard = "";

   bool Visible = true;
   bool BottomClockwise;
   bool FrontClockwise;

   static int moduleIdCounter = 1;
   int moduleId;
   private bool moduleSolved;

   void Awake () {
      moduleId = moduleIdCounter++;

      HideButton.OnInteract += delegate () { StartCoroutine(HidePlanet()); return false; };

      Jupter.OnInteract += delegate () { JupterPress(); return false; };

   }

   void Start () {
      BottomClockwise = Random.Range(0, 2) == 0 ? true : false;
      Debug.LogFormat("[Jupiter #{0}] From the bottom, Jupiter is rotating {1}.", moduleId, BottomClockwise ? "clockwise" : "counter-clockwise");
      FrontClockwise = Random.Range(0, 2) == 0 ? true : false;
      Debug.LogFormat("[Jupiter #{0}] From the front, Jupiter is rotating {1}.", moduleId, FrontClockwise ? "clockwise" : "counter-clockwise");
      Coroutine PlanetRotating = StartCoroutine(PlanetRotation());
      CurrentNumber = Bomb.GetSerialNumberNumbers().ToArray().Sum();
      RotationAmount = Random.Range(0, 4);
      StatusLightPosition.transform.localEulerAngles += new Vector3(0, (float) (90 * RotationAmount), 0);
      string[] TempLog = { "top right", "bottom right", "bottom left", "top left" };
      Debug.LogFormat("[Jupiter #{0}] The status light is {1}.", moduleId, TempLog[RotationAmount]);
      switch (RotationAmount) {
         case 0:
            Debug.LogFormat("[Jupiter #{0}] Starting at F1.", moduleId);
            F1();
            break;
         case 1:
            Debug.LogFormat("[Jupiter #{0}] Starting at F3.", moduleId);
            F3();
            break;
         case 2:
            Debug.LogFormat("[Jupiter #{0}] Starting at B3.", moduleId);
            B3();
            break;
         case 3:
            Debug.LogFormat("[Jupiter #{0}] Starting at B1.", moduleId);
            B1();
            break;
      }
   }

   void JupterPress () {
      if ((int) Bomb.GetTime() % 10 == Math.Abs(CurrentNumber % 10)) {
         Debug.LogFormat("[Jupiter #{0}] You pressed on the correct time, module disarmed.", moduleId);
         GetComponent<KMBombModule>().HandlePass();
         moduleSolved = true;
      }
      else {
         Debug.LogFormat("[Jupiter #{0}] You pressed on a {1}, that is incorrect.", moduleId, (int) Bomb.GetTime() % 10);
         GetComponent<KMBombModule>().HandleStrike();
      }
   }

   #region Animations

   private IEnumerator PlanetRotation () {
      var elapsed = 90f;
      var YElapsed = 90f; //So Jupiter starts in a correct orientation
                          /* big if*/
      while (!moduleSolved) {
         Planet.transform.localEulerAngles = new Vector3(BottomClockwise ? elapsed / 285 * 360 : -elapsed / 285 * 360, FrontClockwise ? YElapsed / 285 * 360 : -YElapsed / 285 * 360, 90f); //depends on time it takes to go around in 1 day
         yield return null;
         elapsed += Time.deltaTime;
         YElapsed += Time.deltaTime;
      }
   }

   private IEnumerator HidePlanet () {
      for (int i = 0; i < 25; i++) {
         yield return new WaitForSecondsRealtime(0.05f);
         Background.transform.localScale += new Vector3(0f, 0.02f, 0f); //depends on size of the planet
      }
      Visible = !Visible;
      Planet.SetActive(Visible);
      for (int i = 0; i < 25; i++) {
         yield return new WaitForSecondsRealtime(0.05f);
         Background.transform.localScale -= new Vector3(0f, 0.02f, 0f); //see above
      }
      yield return null;
   }
   #endregion

   #region Calculation

   bool Calculate (string Parameter) { //Calculates each condition as needed
      Parameter = Parameter.ToUpper();
      int[] I = { 2, 4, 7, 10, 11 };
      int[] J = { 1, 3, 5, 6, 8 };
      bool Temp = false; // The fact that I have to do this is fucking stupid.
      switch (Parameter) {
         case "A":
            if (Visits % 2 == 0) { //Even nodes visited
               Temp = true;
            }
            break;
         case "B":
            Temp = FrontClockwise; //If it rotates clockwise from the front
            break;
         case "C":
            Temp = BottomClockwise; //If it rotates clockwise from the back
            break;
         case "D":
            if (Visits % 2 == 1) { //Odd visits
               Temp = true;
            }
            break;
         case "E":
            if (RotationAmount > 1) { //Left half
               Temp = true;
            }
            break;
         case "F":
            if (RotationAmount < 2) { //Right half
               Temp = true;
            }
            break;
         case "G":
            if (RotationAmount % 3 == 0) { //Top half
               Temp = true;
            }
            break;
         case "H":
            if (RotationAmount % 3 != 0) { //Bottom half
               Temp = true;
            }
            break;
         case "I":
            if (I.Contains(Visits)) {
               Temp = true;
            }
            break;
         case "J":
            if (J.Contains(Visits)) {
               Temp = true;
            }
            break;
         case "K":
            if ((Visits == 9 && !"A2B1C2C3D1E1E2F1G2".Contains(PreviousNode)) || (!(Visits == 9) && !"A2B1C2C3D1E1E2F1G2".Contains(PreviousNode))) {
               Temp = true;
            }
            break;
      }
      return Temp;
   }

   string PositionOfLastOrbital (string Parameter) {
      string Temp = "";
      switch (Parameter) {
         case "A1":
         case "A3":
         case "E1":
         case "E3":
            Temp = "TL";
            break;
         case "A2":
         case "A4":
         case "E2":
         case "E4":
            Temp = "BL";
            break;
         case "C1":
         case "C3":
         case "G1":
         case "G3":
            Temp = "TR";
            break;
         case "C2":
         case "C4":
         case "G2":
         case "G4":
            Temp = "BR";
            break;
         default:
            switch (RotationAmount) {
               case 0:
                  Temp = "BR";
                  break;
               case 1:
                  Temp = "BL";
                  break;
               case 2:
                  Temp = "TL";
                  break;
               case 3:
                  Temp = "TR";
                  break;
            }
            break;
      }
      return Temp;
   }

   void A1 () {
      Visits++;
      if (CurrentNumber > 0) {
         CurrentNumber &= 9;
      }
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at A1.", moduleId, CurrentNumber, Visits);
      if (Calculate("G")) {
         PreviousNode = PreviousOrbital = "A1";
         Debug.LogFormat("[Jupiter #{0}] Going to F3.", moduleId);
         F3();
      }
      else if (Calculate("A")) {
         PreviousNode = PreviousOrbital = "A1";
         if (PreviousConnector == "D1") {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
      }
      else if (Calculate("H")) {
         PreviousNode = PreviousOrbital = "A1";
         if (PreviousConnector == "B2") {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "A1";
         Debug.LogFormat("[Jupiter #{0}] Going to A2.", moduleId);
         A2();
      }
   }

   void A2 () {
      Visits++;
      CurrentNumber += 7;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at A2.", moduleId, CurrentNumber, Visits);
      if (Calculate("F")) {
         PreviousNode = PreviousOrbital = "A2";
         Debug.LogFormat("[Jupiter #{0}] Going to F1.", moduleId);
         F1();
      }
      else if (Calculate("J")) {
         PreviousNode = PreviousOrbital = "A2";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
      }
      else if (Calculate("K")) {
         PreviousNode = PreviousOrbital = "A2";
         if (PreviousConnector == "B2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            B2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            D3();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "A2";
         Debug.LogFormat("[Jupiter #{0}] Going to C2.", moduleId);
         C2();
      }
   }

   void A3 () {
      Visits++;
      CurrentNumber *= 7;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at A3.", moduleId, CurrentNumber, Visits);
      if (Calculate("D")) {
         PreviousNode = PreviousOrbital = "A3";
         Debug.LogFormat("[Jupiter #{0}] Going to B1.", moduleId);
         B1();
      }
      else if (Calculate("E")) {
         PreviousNode = PreviousOrbital = "A3";
         if (PreviousConnector == "B2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
      }
      else if (Calculate("F")) {
         PreviousNode = PreviousOrbital = "A3";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "A3";
         Debug.LogFormat("[Jupiter #{0}] Going to A4.", moduleId);
         A4();
      }
   }

   void A4 () {
      Visits++;
      CurrentNumber += 522;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at A4.", moduleId, CurrentNumber, Visits);
      if (Calculate("A")) {
         PreviousNode = PreviousOrbital = "A4";
         Debug.LogFormat("[Jupiter #{0}] Going to F3.", moduleId);
         F3();
      }
      else if (Calculate("D")) {
         PreviousNode = PreviousOrbital = "A4";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
      }
      else if (Calculate("H")) {
         PreviousNode = PreviousOrbital = "A4";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "A4";
         Debug.LogFormat("[Jupiter #{0}] Going to E4.", moduleId);
         C4();
      }
   }

   void B1 () {
      Visits++;
      CurrentNumber *= 3;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at B1.", moduleId, CurrentNumber, Visits);
      if (Calculate("A")) {
         PreviousStandard = PreviousNode = "B1";
         switch (PositionOfLastOrbital(PreviousOrbital)) {
            case "TL":
               Debug.LogFormat("[Jupiter #{0}] Going to C1.", moduleId);
               C1();
               break;
            case "BL":
               Debug.LogFormat("[Jupiter #{0}] Going to A1.", moduleId);
               A1();
               break;
            case "BR":
               Debug.LogFormat("[Jupiter #{0}] Going to A2.", moduleId);
               A2();
               break;
            case "TR":
               Debug.LogFormat("[Jupiter #{0}] Going to C2.", moduleId);
               C2();
               break;
         }
      }
      else if (Calculate("F")) {
         PreviousStandard = PreviousNode = "B1";
         if (PreviousConnector == "D1") {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
      }
      else if (Calculate("H")) {
         PreviousStandard = PreviousNode = "B1";
         B3();
      }
      else {
         PreviousStandard = PreviousNode = "B1";
         if (PreviousConnector == "B2") {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
      }
   }

   void B2 () {
      Visits++;
      CurrentNumber *= CurrentNumber;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at B2.", moduleId, CurrentNumber, Visits);
      if (Calculate("E")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else if (Calculate("B")) {
         PreviousNode = PreviousConnector = "B2";
         Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
         D1();
      }
      else if (Calculate("K")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else if (Calculate("C")) {
         PreviousNode = PreviousConnector = "B2";
         Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
         D3();
      }
      else if (Calculate("I")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else {
         PreviousNode = PreviousConnector = "B2";
         switch (PreviousStandard) {
            case "B1":
               Debug.LogFormat("[Jupiter #{0}] Going to A1.", moduleId);
               A1();
               break;
            case "B3":
               Debug.LogFormat("[Jupiter #{0}] Going to A3.", moduleId);
               A3();
               break;
            case "F3":
               Debug.LogFormat("[Jupiter #{0}] Going to E3.", moduleId);
               E3();
               break;
            case "F1":
               Debug.LogFormat("[Jupiter #{0}] Going to E1.", moduleId);
               E1();
               break;
         }
      }
   }

   void B3 () {
      Visits++;
      if (CurrentNumber >= 0) {
         CurrentNumber |= 1;
      }
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at B3.", moduleId, CurrentNumber, Visits);
      if (Calculate("C")) {
         PreviousStandard = PreviousNode = "B3";
         switch (PositionOfLastOrbital(PreviousOrbital)) {
            case "TL":
               Debug.LogFormat("[Jupiter #{0}] Going to A4.", moduleId);
               A4();
               break;
            case "BL":
               Debug.LogFormat("[Jupiter #{0}] Going to C4.", moduleId);
               C4();
               break;
            case "BR":
               Debug.LogFormat("[Jupiter #{0}] Going to C3.", moduleId);
               C3();
               break;
            case "TR":
               Debug.LogFormat("[Jupiter #{0}] Going to A3.", moduleId);
               A3();
               break;
         }
      }
      else if (Calculate("E")) {
         PreviousStandard = PreviousNode = "B3";
         if (PreviousConnector == "B2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
      }
      else if (Calculate("H")) {
         PreviousStandard = PreviousNode = "B3";
         F3();
      }
      else {
         PreviousStandard = PreviousNode = "B3";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
      }
   }

   void C1 () {
      Visits++;
      CurrentNumber *= 7;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at C1.", moduleId, CurrentNumber, Visits);
      if (Calculate("D")) {
         PreviousNode = PreviousOrbital = "C1";
         Debug.LogFormat("[Jupiter #{0}] Going to B3.", moduleId);
         B3();
      }
      else if (Calculate("E")) {
         PreviousNode = PreviousOrbital = "C1";
         if (PreviousConnector == "D1") {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
      }
      else if (Calculate("F")) {
         PreviousNode = PreviousOrbital = "C1";
         if (PreviousConnector == "B2") {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "C1";
         Debug.LogFormat("[Jupiter #{0}] Going to A1.", moduleId);
         A1();
      }
   }

   void C2 () {
      Visits++;
      CurrentNumber -= 2;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at C2.", moduleId, CurrentNumber, Visits);
      if (Calculate("B")) {
         PreviousNode = PreviousOrbital = "C2";
         Debug.LogFormat("[Jupiter #{0}] Going to F1.", moduleId);
         F1();
      }
      else if (Calculate("G")) {
         PreviousNode = PreviousOrbital = "C2";
         if (PreviousConnector == "D1") {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
      }
      else if (Calculate("J")) {
         PreviousNode = PreviousOrbital = "C2";
         if (PreviousConnector == "B2") {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "C2";
         Debug.LogFormat("[Jupiter #{0}] Going to C1.", moduleId);
         C1();
      }
   }

   void C3 () {
      Visits++;
      CurrentNumber /= 4;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at C3.", moduleId, CurrentNumber, Visits);
      if (Calculate("C")) {
         PreviousNode = PreviousOrbital = "C3";
         Debug.LogFormat("[Jupiter #{0}] Going to E2.", moduleId);
         E2();
      }
      else if (Calculate("D")) {
         PreviousNode = PreviousOrbital = "C3";
         if (PreviousConnector == "B2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
      }
      else if (Calculate("K")) {
         PreviousNode = PreviousOrbital = "C3";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "C3";
         Debug.LogFormat("[Jupiter #{0}] Going to A3.", moduleId);
         A3();
      }
   }

   void C4 () {
      Visits++;
      if (CurrentNumber >= 0) {
         CurrentNumber |= 11;
      }
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at C4.", moduleId, CurrentNumber, Visits);
      if (Calculate("C")) {
         PreviousNode = PreviousOrbital = "C4";
         Debug.LogFormat("[Jupiter #{0}] Going to F3.", moduleId);
         F3();
      }
      else if (Calculate("O")) {
         PreviousNode = PreviousOrbital = "C4";
         if (PreviousConnector == "B2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
      }
      else if (Calculate("K")) {
         PreviousNode = PreviousOrbital = "C4";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
            B2();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "C4";
         Debug.LogFormat("[Jupiter #{0}] Going to C3.", moduleId);
         C3();
      }
   }

   void D1 () {
      Visits++;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at D1.", moduleId, CurrentNumber, Visits);
      if (Calculate("F")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else if (Calculate("D")) {
         PreviousNode = PreviousConnector = "D1";
         Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
         F2();
      }
      else if (Calculate("K")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else if (Calculate("B")) {
         PreviousNode = PreviousConnector = "D1";
         Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
         B2();
      }
      else if (Calculate("E")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else {
         PreviousNode = PreviousConnector = "D1";
         switch (PreviousStandard) {
            case "B1":
               Debug.LogFormat("[Jupiter #{0}] Going to A1.", moduleId);
               A1();
               break;
            case "B3":
               Debug.LogFormat("[Jupiter #{0}] Going to A3.", moduleId);
               A3();
               break;
            case "F3":
               Debug.LogFormat("[Jupiter #{0}] Going to E3.", moduleId);
               E3();
               break;
            case "F1":
               Debug.LogFormat("[Jupiter #{0}] Going to E1.", moduleId);
               E1();
               break;
         }
      }
   }

   void Goal () {
      Visits++;
      if (CurrentNumber > 0) {
         CurrentNumber %= 10;
      }
      Debug.LogFormat("[Jupiter #{0}] You have visited the goal node, ending with {1} in {2} visits.", moduleId, CurrentNumber, Visits, Visits);
   }

   void D3 () {
      Visits++;
      CurrentNumber += 11;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at D3.", moduleId, CurrentNumber, Visits);
      if (Calculate("K")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else if (Calculate("G")) {
         PreviousNode = PreviousConnector = "D3";
         Debug.LogFormat("[Jupiter #{0}] Going to B2.", moduleId);
         B2();
      }
      else if (Calculate("J")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else if (Calculate("E")) {
         PreviousNode = PreviousConnector = "D3";
         Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
         F2();
      }
      else if (Calculate("F")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else {
         PreviousNode = PreviousConnector = "D3";
         switch (PreviousStandard) {
            case "B1":
               Debug.LogFormat("[Jupiter #{0}] Going to A1.", moduleId);
               A1();
               break;
            case "B3":
               Debug.LogFormat("[Jupiter #{0}] Going to A3.", moduleId);
               A3();
               break;
            case "F3":
               Debug.LogFormat("[Jupiter #{0}] Going to E3.", moduleId);
               E3();
               break;
            case "F1":
               Debug.LogFormat("[Jupiter #{0}] Going to E1.", moduleId);
               E1();
               break;
         }
      }
   }

   void E1 () {
      Visits++;
      CurrentNumber -= 2;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at E1.", moduleId, CurrentNumber, Visits);
      if (Calculate("B")) {
         PreviousNode = PreviousOrbital = "E1";
         Debug.LogFormat("[Jupiter #{0}] Going to B1.", moduleId);
         B1();
      }
      else if (Calculate("G")) {
         PreviousNode = PreviousOrbital = "E1";
         if (PreviousConnector == "F2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else if (Calculate("J")) {
         PreviousNode = PreviousOrbital = "E1";
         if (PreviousConnector == "D1") {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "E1";
         Debug.LogFormat("[Jupiter #{0}] Going to E2.", moduleId);
         E2();
      }
   }

   void E2 () {
      Visits++;
      CurrentNumber /= 4;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at E2.", moduleId, CurrentNumber, Visits);
      if (Calculate("C")) {
         PreviousNode = PreviousOrbital = "E2";
         Debug.LogFormat("[Jupiter #{0}] Going to C3.", moduleId);
         C3();
      }
      else if (Calculate("D")) {
         PreviousNode = PreviousOrbital = "E2";
         if (PreviousConnector == "F2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else if (Calculate("K")) {
         PreviousNode = PreviousOrbital = "E2";
         if (PreviousConnector == "D1") {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "E2";
         Debug.LogFormat("[Jupiter #{0}] Going to G2.", moduleId);
         G2();
      }
   }

   void E3 () {
      Visits++;
      if (CurrentNumber > 0) {
         CurrentNumber %= 5;
      }
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at G1.", moduleId, CurrentNumber, Visits);
      if (Calculate("J")) {
         PreviousNode = PreviousOrbital = "E3";
         Debug.LogFormat("[Jupiter #{0}] Going to F1.", moduleId);
         F1();
      }
      else if (Calculate("D")) {
         PreviousNode = PreviousOrbital = "E3";
         if (PreviousConnector == "F2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else if (Calculate("A")) {
         PreviousNode = PreviousOrbital = "E3";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "E3";
         Debug.LogFormat("[Jupiter #{0}] Going to E4.", moduleId);
         E4();
      }
   }

   void E4 () {
      Visits++;
      if (CurrentNumber >= 0) {
         CurrentNumber |= 11;
      }
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at E4.", moduleId, CurrentNumber, Visits);
      if (Calculate("C")) {
         PreviousNode = PreviousOrbital = "E4";
         Debug.LogFormat("[Jupiter #{0}] Going to B3.", moduleId);
         B3();
      }
      else if (Calculate("O")) {
         PreviousNode = PreviousOrbital = "E4";
         if (PreviousConnector == "F2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else if (Calculate("K")) {
         PreviousNode = PreviousOrbital = "E4";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "E4";
         Debug.LogFormat("[Jupiter #{0}] Going to G4.", moduleId);
         G4();
      }
   }

   void F1 () {
      Visits++;
      CurrentNumber = (int) ((Double) CurrentNumber * 1.5);
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at F1.", moduleId, CurrentNumber, Visits);
      if (Calculate("H")) {
         PreviousStandard = PreviousNode = "F1";
         switch (PositionOfLastOrbital(PreviousOrbital)) {
            case "TL":
               Debug.LogFormat("[Jupiter #{0}] Going to G1.", moduleId);
               G1();
               break;
            case "BL":
               Debug.LogFormat("[Jupiter #{0}] Going to E1.", moduleId);
               E1();
               break;
            case "BR":
               Debug.LogFormat("[Jupiter #{0}] Going to E2.", moduleId);
               E2();
               break;
            case "TR":
               Debug.LogFormat("[Jupiter #{0}] Going to G2.", moduleId);
               G2();
               break;
         }
      }
      else if (Calculate("B")) {
         PreviousStandard = PreviousNode = "F1";
         if (PreviousConnector == "F2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else if (Calculate("C")) {
         PreviousStandard = PreviousNode = "F1";
         Debug.LogFormat("[Jupiter #{0}] Going to B1.", moduleId);
         B1();
      }
      else {
         PreviousStandard = PreviousNode = "F1";
         if (PreviousConnector == "D1") {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
   }

   void F2 () {
      Visits++;
      if (CurrentNumber > 0) {
         CurrentNumber %= 9;
      }
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at F2.", moduleId, CurrentNumber, Visits);
      if (Calculate("K")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else if (Calculate("G")) {
         PreviousNode = PreviousConnector = "F2";
         Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
         D3();
      }
      else if (Calculate("J")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else if (Calculate("E")) {
         PreviousNode = PreviousConnector = "F2";
         Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
         D1();
      }
      else if (Calculate("F")) {
         Debug.LogFormat("[Jupiter #{0}] Going to goal.", moduleId);
         Goal();
      }
      else {
         PreviousNode = PreviousConnector = "F2";
         switch (PreviousStandard) {
            case "B1":
               Debug.LogFormat("[Jupiter #{0}] Going to A1.", moduleId);
               A1();
               break;
            case "B3":
               Debug.LogFormat("[Jupiter #{0}] Going to A3.", moduleId);
               A3();
               break;
            case "F3":
               Debug.LogFormat("[Jupiter #{0}] Going to E3.", moduleId);
               E3();
               break;
            case "F1":
               Debug.LogFormat("[Jupiter #{0}] Going to E1.", moduleId);
               E1();
               break;
         }
      }
   }

   void F3 () {
      Visits++;
      if (CurrentNumber >= 0) {
         CurrentNumber |= 1;
      }
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at F3.", moduleId, CurrentNumber, Visits);
      if (Calculate("C")) {
         PreviousStandard = PreviousNode = "F3";
         switch (PositionOfLastOrbital(PreviousOrbital)) {
            case "TL":
               Debug.LogFormat("[Jupiter #{0}] Going to G3.", moduleId);
               G3();
               break;
            case "BL":
               Debug.LogFormat("[Jupiter #{0}] Going to E3.", moduleId);
               E3();
               break;
            case "BR":
               Debug.LogFormat("[Jupiter #{0}] Going to E4.", moduleId);
               E4();
               break;
            case "TR":
               Debug.LogFormat("[Jupiter #{0}] Going to G4.", moduleId);
               G4();
               break;
         }
      }
      else if (Calculate("E")) {
         PreviousStandard = PreviousNode = "F3";
         if (PreviousConnector == "F2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else if (Calculate("H")) {
         PreviousStandard = PreviousNode = "F3";
         F1();
      }
      else {
         PreviousStandard = PreviousNode = "F3";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
   }

   void G1 () {
      Visits++;
      if (CurrentNumber > 0) {
         CurrentNumber %= 5;
      }
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at G1.", moduleId, CurrentNumber, Visits);
      if (Calculate("J")) {
         PreviousNode = PreviousOrbital = "G1";
         Debug.LogFormat("[Jupiter #{0}] Going to F3.", moduleId);
         F3();
      }
      else if (Calculate("D")) {
         PreviousNode = PreviousOrbital = "G1";
         if (PreviousConnector == "F2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else if (Calculate("A")) {
         PreviousNode = PreviousOrbital = "G1";
         if (PreviousConnector == "D1") {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "G1";
         Debug.LogFormat("[Jupiter #{0}] Going to E1.", moduleId);
         E1();
      }
   }

   void G2 () {
      Visits++;
      CurrentNumber += 7;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at G2.", moduleId, CurrentNumber, Visits);
      if (Calculate("F")) {
         PreviousNode = PreviousOrbital = "G2";
         Debug.LogFormat("[Jupiter #{0}] Going to B1.", moduleId);
         B1();
      }
      else if (Calculate("J")) {
         PreviousNode = PreviousOrbital = "G2";
         if (PreviousConnector == "D1") {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
      }
      else if (Calculate("K")) {
         PreviousNode = PreviousOrbital = "G2";
         if (PreviousConnector == "F2") {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to D1.", moduleId);
            D1();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "G2";
         Debug.LogFormat("[Jupiter #{0}] Going to G1.", moduleId);
         G1();
      }
   }

   void G3 () {
      Visits++;
      if (CurrentNumber > 0) {
         CurrentNumber &= 9;
      }
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at G3.", moduleId, CurrentNumber, Visits);
      if (Calculate("G")) {
         PreviousNode = PreviousOrbital = "G3";
         Debug.LogFormat("[Jupiter #{0}] Going to B1.", moduleId);
         B1();
      }
      else if (Calculate("A")) {
         PreviousNode = PreviousOrbital = "G3";
         if (PreviousConnector == "F2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else if (Calculate("H")) {
         PreviousNode = PreviousOrbital = "G3";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "G3";
         Debug.LogFormat("[Jupiter #{0}] Going to E3.", moduleId);
         E3();
      }
   }

   void G4 () {
      Visits++;
      CurrentNumber += 522;
      if (Visits == 12) {
         Debug.LogFormat("[Jupiter #{0}] Current visit total is twelve, ending movement with a final value of {1}.", moduleId, CurrentNumber, Visits);
         return;
      }
      Debug.LogFormat("[Jupiter #{0}] At the {2}th visit, the current number is {1} at G4.", moduleId, CurrentNumber, Visits);
      if (Calculate("A")) {
         PreviousNode = PreviousOrbital = "G4";
         Debug.LogFormat("[Jupiter #{0}] Going to B3.", moduleId);
         B3();
      }
      else if (Calculate("D")) {
         PreviousNode = PreviousOrbital = "G4";
         if (PreviousConnector == "F2") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else if (Calculate("H")) {
         PreviousNode = PreviousOrbital = "G4";
         if (PreviousConnector == "D3") {
            Debug.LogFormat("[Jupiter #{0}] Going to D3.", moduleId);
            D3();
         }
         else {
            Debug.LogFormat("[Jupiter #{0}] Going to F2.", moduleId);
            F2();
         }
      }
      else {
         PreviousNode = PreviousOrbital = "G4";
         Debug.LogFormat("[Jupiter #{0}] Going to G3.", moduleId);
         G3();
      }
   }

   #endregion

   #region Twitch Shit

#pragma warning disable 414
   private readonly string TwitchHelpMessage = @"Use !{0} # to press Jupiter on that time.";
#pragma warning restore 414

   IEnumerator ProcessTwitchCommand (string Command) {
      yield return null;
      Command = Command.Trim();
      if (Command.Length != 1 || !"0123456789".Contains(Command)) {
         yield return "sendtochaterror I don't understand!";
      }
      else {
         while ((int) Bomb.GetTime() % 10 != int.Parse(Command)) {
            yield return null;
         }
         Jupter.OnInteract();
      }
   }

   IEnumerator TwitchHandleForcedSolve () {
      while (!moduleSolved) {
         while ((int) Bomb.GetTime() % 10 != Math.Abs(CurrentNumber % 10)) {
            yield return true;
         }
         Jupter.OnInteract();
      }
   }
   #endregion
}
