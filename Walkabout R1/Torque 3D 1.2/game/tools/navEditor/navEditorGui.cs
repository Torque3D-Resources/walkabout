//-----------------------------------------------------------------------------
// Copyright (c) Daniel Buckmaster 2012
//-----------------------------------------------------------------------------

$Nav::EditorOpen = false;

function NavEditorGui::onEditorActivated(%this)
{
   // When the editor is activated, the last NavMesh selected remains so. All
   // other objects are deselected.
   %count = EWorldEditor.getSelectionSize();
   %nm = "";
   while(%count)
   {
      %obj = EWorldEditor.getSelectedObject(0);
      if(%obj.getClassName() $= "NavMesh")
         %nm = %obj;
      EWorldEditor.unselectObject(%obj);
      %count--;
   }
   if(isObject(%nm))
      %this.selectObject(%nm);
   %this.prepSelectionMode();
}

function NavEditorGui::onEditorDeactivated(%this)
{
}

function NavEditorGui::onModeSet(%this, %mode)
{
   // Callback when the nav editor changes mode. Set the appropriate dynamic
   // GUI contents in the properties/actions boxes.
   NavInspector.setVisible(false);

   %actions = NavEditorOptionsWindow->ActionsBox;
   %actions->SelectActions.setVisible(false);
   %actions->LinkActions.setVisible(false);
   %actions->CoverActions.setVisible(false);
   %actions->TileActions.setVisible(false);
   %actions->TestActions.setVisible(false);

   %properties = NavEditorOptionsWindow->PropertiesBox;
   %properties->LinkProperties.setVisible(false);
   %properties->TileProperties.setVisible(false);
   %properties->TestProperties.setVisible(false);

   switch$(%mode)
   {
   case "SelectMode":
      NavInspector.setVisible(true);
      %actions->SelectActions.setVisible(true);
   case "LinkMode":
      %actions->LinkActions.setVisible(true);
      %properties->LinkProperties.setVisible(true);
   case "CoverMode":
      // 
      %actions->CoverActions.setVisible(true);
   case "TileMode":
      %actions->TileActions.setVisible(true);
      %properties->TileProperties.setVisible(true);
   case "TestMode":
      %actions->TestActions.setVisible(true);
      %properties->TestProperties.setVisible(true);
   }
}

function NavEditorGui::paletteSync(%this, %mode)
{
   // Synchronise the palette (small buttons on the left) with the actual mode
   // the nav editor is in.
   %evalShortcut = "ToolsPaletteArray-->" @ %mode @ ".setStateOn(1);";
   eval(%evalShortcut);
} 

function NavEditorGui::onEscapePressed(%this)
{
   return false;
}

function NavEditorGui::selectObject(%this, %obj)
{
   NavTreeView.clearSelection();
   if(isObject(%obj))
      NavTreeView.selectItem(%obj);
   %this.onObjectSelected(%obj);
}

function NavEditorGui::onObjectSelected(%this, %obj)
{
   if(%this.selectedObject == %obj)
      return;
   if(isObject(%this.selectedObject))
      %this.deselect();
   %this.selectedObject = %obj;
   if(isObject(%obj))
      %this.selectMesh(%obj);
}

function NavEditorGui::deleteMesh(%this)
{
   if(isObject(%this.selectedObject))
   {
      %this.selectedObject.delete();
      %this.selectObject(-1);
   }
}

function NavEditorGui::deleteSelected(%this)
{
   switch$(%this.getMode())
   {
   case "SelectMode":
      // Try to delete the selected NavMesh.
      if(isObject(NavEditorGui.selectedObject))
         MessageBoxYesNo("Warning",
            "Are you sure you want to delete this NavMesh?",
            "NavEditorGui.deleteMesh();");
   case "TestMode":
      %this.getPlayer().delete();
      %this.onPlayerDeselected();
   case "LinkMode":
      %this.deleteLink();
      %this.isDirty = true;
   }
}

function NavEditorGui::buildSelectedMeshes(%this)
{
   if(isObject(%this.getMesh()))
   {
      %this.getMesh().build();
      %this.isDirty = true;
   }
}

function NavEditorGui::buildLinks(%this)
{
   if(isObject(%this.getMesh()))
   {
      %this.getMesh().buildLinks();
      %this.isDirty = true;
   }
}

function updateLinkData(%control, %flags)
{
   %control->LinkWalkFlag.setActive(true);
   %control->LinkJumpFlag.setActive(true);
   %control->LinkDropFlag.setActive(true);
   %control->LinkLedgeFlag.setActive(true);
   %control->LinkClimbFlag.setActive(true);
   %control->LinkTeleportFlag.setActive(true);

   %control->LinkWalkFlag.setStateOn(%flags & $Nav::WalkFlag);
   %control->LinkJumpFlag.setStateOn(%flags & $Nav::JumpFlag);
   %control->LinkDropFlag.setStateOn(%flags & $Nav::DropFlag);
   %control->LinkLedgeFlag.setStateOn(%flags & $Nav::LedgeFlag);
   %control->LinkClimbFlag.setStateOn(%flags & $Nav::ClimbFlag);
   %control->LinkTeleportFlag.setStateOn(%flags & $Nav::TeleportFlag);
}

function getLinkFlags(%control)
{
   return (%control->LinkWalkFlag.isStateOn() ? $Nav::WalkFlag : 0) |
          (%control->LinkJumpFlag.isStateOn() ? $Nav::JumpFlag : 0) |
          (%control->LinkDropFlag.isStateOn() ? $Nav::DropFlag : 0) |
          (%control->LinkLedgeFlag.isStateOn() ? $Nav::LedgeFlag : 0) |
          (%control->LinkClimbFlag.isStateOn() ? $Nav::ClimbFlag : 0) |
          (%control->LinkTeleportFlag.isStateOn() ? $Nav::TeleportFlag : 0);
}

function disableLinkData(%control)
{
   %control->LinkWalkFlag.setActive(false);
   %control->LinkJumpFlag.setActive(false);
   %control->LinkDropFlag.setActive(false);
   %control->LinkLedgeFlag.setActive(false);
   %control->LinkClimbFlag.setActive(false);
   %control->LinkTeleportFlag.setActive(false);
}

function NavEditorGui::onLinkSelected(%this, %flags)
{
   updateLinkData(NavEditorOptionsWindow-->LinkProperties, %flags);
}

function NavEditorGui::onPlayerSelected(%this, %flags)
{
   updateLinkData(NavEditorOptionsWindow-->TestProperties, %flags);
}

function NavEditorGui::updateLinkFlags(%this)
{
   if(isObject(%this.getMesh()))
   {
      %properties = NavEditorOptionsWindow-->LinkProperties;
      %this.setLinkData(getLinkFlags(%properties));
      %this.isDirty = true;
   }
}

function NavEditorGui::updateTestFlags(%this)
{
   if(isObject(%this.getPlayer()))
   {
      %properties = NavEditorOptionsWindow-->TestProperties;
      %player = %this.getPlayer();

      %player.allowWwalk = %properties->LinkWalkFlag.isStateOn();
      %player.allowJump = %properties->LinkJumpFlag.isStateOn();
      %player.allowDrop = %properties->LinkDropFlag.isStateOn();
      %player.allowLedge = %properties->LinkLedgeFlag.isStateOn();
      %player.allowClimb = %properties->LinkClimbFlag.isStateOn();
      %player.allowTeleport = %properties->LinkTeleportFlag.isStateOn();

      %this.isDirty = true;
   }
}

function NavEditorGui::onLinkDeselected(%this)
{
   disableLinkData(NavEditorOptionsWindow-->LinkProperties);
}

function NavEditorGui::onPlayerDeselected(%this)
{
   disableLinkData(NavEditorOptionsWindow-->TestProperties);
}

function NavEditorGui::createCoverPoints(%this)
{
   if(isObject(%this.getMesh()))
   {
      %this.getMesh().createCoverPoints();
      %this.isDirty = true;
   }
}

function NavEditorGui::deleteCoverPoints(%this)
{
   if(isObject(%this.getMesh()))
   {
      %this.getMesh().deleteCoverPoints();
      %this.isDirty = true;
   }
}

function NavEditorGui::findCover(%this)
{
   if(%this.getMode() $= "TestMode" && isObject(%this.getPlayer()))
   {
      %pos = LocalClientConnection.getControlObject().getPosition();
      %text = NavEditorOptionsWindow-->TestProperties->CoverPosition.getText();
      if(%text !$= "")
         %pos = eval(%text);
      %this.getPlayer().findCover(%pos, NavEditorOptionsWindow-->TestProperties->CoverRadius.getText());
   }
}

function NavEditorGui::followObject(%this)
{
   if(%this.getMode() $= "TestMode" && isObject(%this.getPlayer()))
   {
      %obj = LocalClientConnection.player;
      %text = NavEditorOptionsWindow-->TestProperties->FollowObject.getText();
      if(%text !$= "")
      {
         eval("%obj = " @ %text);
         if(!isObject(%obj))
            MessageBoxOk("Error", "Cannot find object" SPC %text);
      }
      if(isObject(%obj))
         %this.getPlayer().followObject(%obj, NavEditorOptionsWindow-->TestProperties->FollowRadius.getText());
   }
}

function NavInspector::inspect(%this, %obj)
{
   %name = "";
   if(isObject(%obj))
      %name = %obj.getName();
   else
      NavFieldInfoControl.setText("");

   Parent::inspect(%this, %obj);
}

function NavInspector::onInspectorFieldModified(%this, %object, %fieldName, %arrayIndex, %oldValue, %newValue)
{
   // Same work to do as for the regular WorldEditor Inspector.
   Inspector::onInspectorFieldModified(%this, %object, %fieldName, %arrayIndex, %oldValue, %newValue);
}

function NavInspector::onFieldSelected(%this, %fieldName, %fieldTypeStr, %fieldDoc)
{
   NavFieldInfoControl.setText("<font:ArialBold:14>" @ %fieldName @ "<font:ArialItalic:14> (" @ %fieldTypeStr @ ") " NL "<font:Arial:14>" @ %fieldDoc);
}

function NavTreeView::onInspect(%this, %obj)
{
   NavInspector.inspect(%obj);
}

function NavTreeView::onSelect(%this, %obj)
{
   NavInspector.inspect(%obj);
   NavEditorGui.onObjectSelected(%obj);
}

function NavEditorGui::prepSelectionMode(%this)
{
   %this.setMode("SelectMode");
   ToolsPaletteArray-->NavEditorSelectMode.setStateOn(1);
}

//-----------------------------------------------------------------------------

function ENavEditorPaletteButton::onClick(%this)
{
   // When clicking on a pelette button, add its description to the bottom of
   // the editor window.
   EditorGuiStatusBar.setInfo(%this.DetailedDesc);
}

//-----------------------------------------------------------------------------

function NavMeshLinkFlagButton::onClick(%this)
{
   NavEditorGui.updateLinkFlags();
}

function NavMeshTestFlagButton::onClick(%this)
{
   NavEditorGui.updateTestFlags();
}
