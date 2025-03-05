using System;
using GME.GM;
using Godot;

namespace GME.UI;

public partial class PolygonEditor : Node
{
	// Details
	private LineEdit _handleText;
	private OptionButton _renderTypeOption;
	private CheckBox _mappableCheckbox;
	private CheckBox _slowSortCheckbox;
	
	// Texture
	private TextureRect _textureRect;
	private LineEdit _textureNameText;
	private OptionButton _textureEffectOption;
	
	// Overlay
	private TextureRect _overlayRect;
	private LineEdit _overlayNameText;
	private OptionButton _overlayEffectOption;
	
	// UV Alignment
	private SpinBox _textureU;
	private SpinBox _textureV;
	private SpinBox _lightmapU;
	private SpinBox _lightmapV;
	private SpinBox _uProjectionX;
	private SpinBox _uProjectionY;
	private SpinBox _uProjectionZ;
	private SpinBox _vProjectionX;
	private SpinBox _vProjectionY;
	private SpinBox _vProjectionZ;
	
	// UV Scrollling
	private SpinBox _textureScrollU;
	private SpinBox _textureScrollV;
	private SpinBox _lightmapScrollU;
	private SpinBox _lightmapScrollV;
	
	// Used to prevent triggering object rebuilds
	private bool _changingSelection;
	
	public override void _Ready()
	{
		_handleText = GetNode<LineEdit>("%HandleText");
		_renderTypeOption = GetNode<OptionButton>("%RenderTypeOption");
		_mappableCheckbox = GetNode<CheckBox>("%MappableCheckbox");
		_slowSortCheckbox = GetNode<CheckBox>("%SlowSortCheckbox");
		_textureRect = GetNode<TextureRect>("%TextureRect");
		_textureNameText = GetNode<LineEdit>("%TextureNameText");
		_textureEffectOption = GetNode<OptionButton>("%TextureEffectOption");
		_overlayRect = GetNode<TextureRect>("%OverlayRect");
		_overlayNameText = GetNode<LineEdit>("%OverlayNameText");
		_overlayEffectOption = GetNode<OptionButton>("%OverlayEffectOption");
		_textureU = GetNode<SpinBox>("%TextureU");
		_textureV = GetNode<SpinBox>("%TextureV");
		_lightmapU = GetNode<SpinBox>("%LightmapU");
		_lightmapV = GetNode<SpinBox>("%LightmapV");
		_uProjectionX = GetNode<SpinBox>("%UProjectionX");
		_uProjectionY = GetNode<SpinBox>("%UProjectionY");
		_uProjectionZ = GetNode<SpinBox>("%UProjectionZ");
		_vProjectionX = GetNode<SpinBox>("%VProjectionX");
		_vProjectionY = GetNode<SpinBox>("%VProjectionY");
		_vProjectionZ = GetNode<SpinBox>("%VProjectionZ");
		_textureScrollU = GetNode<SpinBox>("%TextureScrollU");
		_textureScrollV = GetNode<SpinBox>("%TextureScrollV");
		_lightmapScrollU = GetNode<SpinBox>("%LightmapScrollU");
		_lightmapScrollV = GetNode<SpinBox>("%LightmapScrollV");
		
		// Disable all the stuff I don't support yet
		_handleText.Editable = false;
		_mappableCheckbox.Disabled = true;
		_slowSortCheckbox.Disabled = true;

		_renderTypeOption.ItemSelected += idx => UpdatePoly(p => p.Mode = (Polygon.RenderMode)idx);
		_textureU.ValueChanged += u => UpdatePoly(p => p.Uv.X = (float)u);
		_textureV.ValueChanged += v => UpdatePoly(p => p.Uv.Y = (float)v);
		_lightmapU.ValueChanged += u => UpdatePoly(p => p.LmUv.X = (float)u);
		_lightmapV.ValueChanged += v => UpdatePoly(p => p.LmUv.Y = (float)v);
		_uProjectionX.ValueChanged += x => UpdatePoly(p => p.UVec.X = (float)x);
		_uProjectionY.ValueChanged += y => UpdatePoly(p => p.UVec.Y = (float)y);
		_uProjectionZ.ValueChanged += z => UpdatePoly(p => p.UVec.Z = (float)z);
		_vProjectionX.ValueChanged += x => UpdatePoly(p => p.VVec.X = (float)x);
		_vProjectionY.ValueChanged += y => UpdatePoly(p => p.VVec.Y = (float)y);
		_vProjectionZ.ValueChanged += z => UpdatePoly(p => p.VVec.Z = (float)z);

		EditorContext.Instance.SelectionChanged += OnSelectionChanged;
	}

	private bool ValidSelection(out Selection selection)
	{
		selection = EditorContext.Instance.CurrentSelection;
		return selection != Selection.None;
	}

	private void OnSelectionChanged()
	{
		_changingSelection = true;
		var context = EditorContext.Instance;
		if (ValidSelection(out var selection))
		{
			var poly = context.World.Sobs[selection.GlobalObjectId].Polygons[selection.PolyId];
			_renderTypeOption.Disabled = false;
			_renderTypeOption.Selected = (int)poly.Mode;
			// _textureNameText.Editable = true;
			_textureNameText.Text = poly.TextureName;
			// _textureEffectOption.Disabled = true;
			// _textureEffectOption.Selected = 0;
			// _overlayNameText.Editable = false;
			// _overlayNameText.Text = "None";
			// _overlayEffectOption.Disabled = true;
			// _overlayEffectOption.Selected = 0;
			_textureU.Editable = true;
			_textureU.Value = poly.Uv.X;
			_textureV.Editable = true;
			_textureV.Value = poly.Uv.Y;
			_lightmapU.Editable = true;
			_lightmapU.Value = poly.LmUv.X;
			_lightmapV.Editable = true;
			_lightmapV.Value = poly.LmUv.Y;
			_uProjectionX.Editable = true;
			_uProjectionX.Value = poly.UVec.X;
			_uProjectionY.Editable = true;
			_uProjectionY.Value = poly.UVec.Y;
			_uProjectionZ.Editable = true;
			_uProjectionZ.Value = poly.UVec.Z;
			_vProjectionX.Editable = true;
			_vProjectionX.Value = poly.VVec.X;
			_vProjectionY.Editable = true;
			_vProjectionY.Value = poly.VVec.Y;
			_vProjectionZ.Editable = true;
			_vProjectionZ.Value = poly.VVec.Z;
			// _textureScrollU.Value = 0.0f;
			// _textureScrollU.Editable = false;
			// _textureScrollV.Value = 0.0f;
			// _textureScrollV.Editable = false;
			// _lightmapScrollU.Value = 0.0f;
			// _lightmapScrollU.Editable = false;
			// _lightmapScrollV.Value = 0.0f;
			// _lightmapScrollV.Editable = false;
		} else
		{
			_renderTypeOption.Selected = -1;
			_renderTypeOption.Disabled = true;
			_textureNameText.Text = "None";
			_textureNameText.Editable = false;
			_textureEffectOption.Selected = 0;
			_textureEffectOption.Disabled = true;
			_overlayNameText.Text = "None";
			_overlayNameText.Editable = false;
			_overlayEffectOption.Selected = 0;
			_overlayEffectOption.Disabled = true;
			_textureU.Value = 0.0f;
			_textureU.Editable = false;
			_textureV.Value = 0.0f;
			_textureV.Editable = false;
			_lightmapU.Value = 0.0f;
			_lightmapU.Editable = false;
			_lightmapV.Value = 0.0f;
			_lightmapV.Editable = false;
			_uProjectionX.Value = 1.0f;
			_uProjectionX.Editable = false;
			_uProjectionY.Value = 0.0f;
			_uProjectionY.Editable = false;
			_uProjectionZ.Value = 0.0f;
			_uProjectionZ.Editable = false;
			_vProjectionX.Value = 0.0f;
			_vProjectionX.Editable = false;
			_vProjectionY.Value = 1.0f;
			_vProjectionY.Editable = false;
			_vProjectionZ.Value = 0.0f;
			_vProjectionZ.Editable = false;
			_textureScrollU.Value = 0.0f;
			_textureScrollU.Editable = false;
			_textureScrollV.Value = 0.0f;
			_textureScrollV.Editable = false;
			_lightmapScrollU.Value = 0.0f;
			_lightmapScrollU.Editable = false;
			_lightmapScrollV.Value = 0.0f;
			_lightmapScrollV.Editable = false;
		}

		_changingSelection = false;
	}

	private void UpdatePoly(Action<Polygon> f)
	{
		if (_changingSelection || !ValidSelection(out var selection))
		{
			return;
		}

		var context = EditorContext.Instance;
		f(context.World.Sobs[selection.GlobalObjectId].Polygons[selection.PolyId]);
		context.TriggerObjectUpdated();
	}
}