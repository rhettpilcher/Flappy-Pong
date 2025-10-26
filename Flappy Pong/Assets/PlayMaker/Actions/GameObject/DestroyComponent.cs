// (c) Copyright HutongGames, LLC 2010-2013. All rights reserved.

using UnityEngine;

namespace HutongGames.PlayMaker.Actions
{
	[ActionCategory(ActionCategory.GameObject)]
	[Tooltip("Destroys a Component of an Object.")]
	public class DestroyComponent : FsmStateAction
	{
		[RequiredField]
        [Tooltip("The GameObject that owns the Component.")]
		public FsmOwnerDefault gameObject;

		[HasFloatSlider(0, 5)]
		[Tooltip("Optional delay before destroying the Game Object.")]
		public FsmFloat delay;

		[RequiredField]
		[UIHint(UIHint.ScriptComponent)]
        [Tooltip("The name of the Component to destroy.")]
		public FsmString component;
				
		Component aComponent;

		public override void Reset()
		{
			delay = 0;
			aComponent = null;
			gameObject = null;
			component = null;
		}

		public override void OnEnter()
		{
			DoDestroyComponent(gameObject.OwnerOption == OwnerDefaultOption.UseOwner ? Owner : gameObject.GameObject.Value);

			Finish();
		}

		
		void DoDestroyComponent(GameObject go)
		{
			aComponent = go.GetComponent(ReflectionUtils.GetGlobalType(component.Value));
			if (aComponent == null)
			{
				LogError("No such component: " + component.Value);
			}
			else
			{
				if (delay.Value > 0)
					Object.Destroy(aComponent, delay.Value);
				else
					Object.Destroy(aComponent);
			}
		}
	}
}