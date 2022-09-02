using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class WrapSpeedControl : MonoBehaviour
{
    public VisualEffect warpSpeedVFX;
    public MeshRenderer cylinder;
    public float rate =0.05f;
    public float delay = 3.0f;

    private bool warpActive;

    // Start is called before the first frame update
    void Start()
    {
        Input.multiTouchEnabled = true;
        warpSpeedVFX.Stop();
        warpSpeedVFX.SetFloat("WarpAmount", 0);

        cylinder.material.SetFloat("_Active",0);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) ||Input.GetMouseButtonDown(0))// || Input.GetMouseButton(0) || Input.touchCount > 0
        {
            warpActive =true;
            StartCoroutine(ActivateParticles());
            StartCoroutine(ActivateShader());
        }
        if(Input.GetKeyUp(KeyCode.Space)||Input.GetMouseButtonUp(0))
        {
            warpActive =false;
            StartCoroutine(ActivateParticles());
            StartCoroutine(ActivateShader());
        }
    }

    IEnumerator ActivateParticles()
    {
        if(warpActive)
        {
            warpSpeedVFX.Play();
            float amount = warpSpeedVFX.GetFloat("WarpAmount");
            while(amount < 1&& warpActive)
            {
                amount += rate;
                warpSpeedVFX.SetFloat("WarpAmount", amount);
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            float amount = warpSpeedVFX.GetFloat("WarpAmount");
            while(amount > 0 && !warpActive)
            {
                amount -= rate;
                warpSpeedVFX.SetFloat("WarpAmount", amount);
                yield return new WaitForSeconds(0.1f);

                if(amount<=0+rate)
                {
                    amount=0;
                    warpSpeedVFX.SetFloat("WarpAmount", amount);
                    warpSpeedVFX.Stop();
                }
            }
        }
    }

    IEnumerator ActivateShader()
    {
        if(warpActive)
        {
            yield return new WaitForSeconds(delay);
            float amount = cylinder.material.GetFloat("_Active");
            while(amount < 1&& warpActive)
            {
                amount += rate;
                cylinder.material.SetFloat("_Active",amount);
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            float amount = cylinder.material.GetFloat("_Active");
            while(amount > 0 && !warpActive)
            {
                amount -= rate;
                cylinder.material.SetFloat("_Active",amount);
                yield return new WaitForSeconds(0.1f);

                if(amount<=0+rate)
                {
                    amount=0;
                    cylinder.material.SetFloat("_Active",amount);
                }
            }
        }
    }
}
