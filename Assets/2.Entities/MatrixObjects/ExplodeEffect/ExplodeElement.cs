using System;
using System.Collections;
using UnityEngine;

public class ExplodeElement : MonoBehaviour
{
    private MatrixObject mo;
    public MatrixObject MO => mo;
    
    ParticleSystem ps;
    
    private Coroutine explodeCoroutine;
    public bool isChainingChicken;
    private void Awake()
    {
        mo = gameObject.GetComponent<MatrixObject>();
        ps = GetComponent<ParticleSystem>();
    }

    

    public void ExplodeCell(bool sourceChainingChicken)
    {
        isChainingChicken = sourceChainingChicken;
        ps.Play();

        explodeCoroutine = StartCoroutine(ExplodeCellCoroutine(sourceChainingChicken));
    }

    IEnumerator ExplodeCellCoroutine(bool isChainingChicken)
    {
        yield return new WaitForSeconds(0.7f);
        
        MatrixCell currentCell = mo.GetCurrentCell();
        
        if (isChainingChicken || isChainingChicken)
        {
            MatrixObject chicken = Instantiate(GamePlayGridManager.Instance.chickenPrefab.GetComponent<MatrixObject>());
            currentCell.PutMatrixObject(chicken);
            currentCell.state = MatrixCell.CellState.Filled;
        }
        else
        {
            currentCell.matrixObject = null;
            currentCell.state = MatrixCell.CellState.Empty;
        }
        
        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject);
    }

    public void CancelChaining()
    {
        if (explodeCoroutine != null) StopCoroutine(explodeCoroutine);
    }
}
