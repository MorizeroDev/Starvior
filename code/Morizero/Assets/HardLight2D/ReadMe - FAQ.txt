HARD LIGHT 2D
-------------
Made by: Trino (Twitter: @Trino_dev)

Contents of this file

   1. What is it?
   2. What does it do?
   3. What does it not do?
   4. How do I use it?
   5. What does X variable do? / How can I tweak it?
   6. How can I get better performance?
   7. Triggers don't cast shadows (IMPORTANT)
   8. My collider shape is not updating for the lights (IMPORTANT)
   9. The shadows coming from Boxes/Circles look weird (IMPORTANT)
   10. I want to use my own shader
   11. How can I hide the lights on the Scene view?
   12. Should it be working while in Edit mode?
   13. There's something wrong / How can I contact you?

-------------

   1. What is it?

      - It is a 2D point light system whose shadows are completely sharp.
      - It is designed to be used with an orthographic camera and coexist with 2D
      renderers (e.g., sprites, lines or tilemaps).


   2. What does it do?

      Every light detects every 2D collider on its range and creates a planar 3D
      mesh around it's position taking the shape of a shadow casting point light.

      Supported colliders:
      - BoxCollider2D
      - CircleCollider2D
      - EdgeCollider2D
      - PolygonCollider2D
      - CompositeCollider2D


   3. What does it not do?

      - It does not darken the scene, the "shadows" are simply the area where the
      light does not reach and will be as dark as your scene is by default.
      - It does not offer any functionality gameplay-wise like fog of war or 
      sending a message to lit colliders.


   4. How do I use it?

      1. Drag the prefab "Light2D" on your scene and drop it wherever you please 
      (The position on the Z axis does not affect the result).
      2. You can safely break the prefab link.
      3. Tweak it's properties.


   5. What does X variable do? / How can I tweak it?

      Every variable on the Inspector view has it's own tooltip, you need only 
      hover your mouse over the variable name to read it.

      The demo scene also features a variety of cases where almost every light has 
      a different setup.


   6. How can I get better performance?

         * Note: If no light or collider is calling for an update, all 
         calculations should automatically stop.
         
      There are some cases where performance will drop:
      - Too many lights moving around.
      - Too many shadow casters (specially complex ones) moving/rotating/scaling 
      within range of a light.
      - Too many shadow casters actively intersecting with each other within range 
      of a light.
      - Too many shadow casters being instantiated frequently.
      - Any combination of the previous cases.

      Consider:
      - Reducing the amount of shadow casters by tweaking the variables under the 
      Filtering Settings.
      - Referencing a camera on the Optimization Settings to enable culling.
      - Enabling Calculate Only Once on the Optimization Settings whenever possible
      (e.g., background/static lights)
      - Breaking big/complex Tilemaps (using CompositeCollider2D) into smaller 
      squarish chunks.


   7. Triggers don't cast shadows (IMPORTANT)

      This was a deliberate choice in the name of statistics and performance.
      One of the reasons being that triggers are generally used to detect other
      objects and not to block physical things such as rigidbodies or light rays.

      It could be enabled as an option on a future update if the demand is high, so
      don't hesitate on letting me know your opinion.


   8. My collider shape is not updating for the lights (IMPORTANT)

         * Note: "Shape" refers to the collider's settings (e.g., the points on a 
         PolygonCollider2D), any change to the Transform should be updated 
         automatically.

      One of the steps taken to optimize this script is to store the initial shape 
      of any collider (The first time it's in range of any light) and base every
      calculation around that. 

      Despite this, it is possible to manually update a collider's shape:
      
      - While in Edit mode, where you are constantly changing colliders (e.g.,
      building a tilemap) you can update every collider by going to
      HardLight2D > "Refresh collider references" in Unity's main menu.

      - While on Play mode you can update a single collider's shape by calling 
      "HardLight2DManager.RefreshColliderReference(yourCollider)" from any script.
      (Keep in mind that this function WILL create memory garbage everytime it's 
      called and may be very heavy depending on the collider's complexity)


   9. The shadows coming from Boxes/Circles look weird (IMPORTANT)

      Either as an optimization step or one of many other reasons, some collider's 
      shadow shape can change depending on it's proximity or angle relative to the 
      light source.
      As long as the collider's sprite cover's it's entire area and no light 
      source goes inside it, it should look natural.


   10. I want to use my own shader
      
      If you want to use a different shader and be able to use the color and 
      intensity properties, all you need is to have your shader's main color 
      property be named "_Color" and tag it with a [PerRendererData] attribute.   


   11. How can I hide the lights on the Scene view?

      Since the prefab uses the "TransparentFX" Layer by default, you can toggle 
      it's scene visibility on the "Layers" dropdown on the top right corner of 
      the Unity window (next to the "Layout" dropdown).


   12. Should it be working while in Edit mode?

         * Note: Some lights may fail to update while on Edit mode. Moving them 
         around a bit should fix that.

      Yes, although it relies on the Gizmo system, meaning there are a couple of 
      scenarios where it won't auto update while on edit mode:
      - Gizmos are toggled off.
      - HardLight2D is disabled on the Gizmos dropdown.
      - The HardLight2D script is minimized on the Inspector window.
      - Light's are hidden in the Scene view.


   13. There's something wrong / How can I contact you?

      You're welcome to write a review on the Asset Store page, although you can 
      always write me a direct message on Twitter (@Trino_dev) for a faster 
      response.
      If you don't use Twitter and still want a faster response than writing on
      the Asset Store page, you can email me at trino.vidya@gmail.com