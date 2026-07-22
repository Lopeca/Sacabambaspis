using System;
using System.Collections;
using UnityEngine;

public class ExplodeElement : MonoBehaviour
{
    private MatrixObject mo;
    public MatrixObject MO => mo;
    
    ParticleSystem ps;

    [SerializeField] private MatrixObject prevObject;
    private ExplodeOnDeath prevObjectExplodeComponent;

    // private Coroutine explodeCoroutine; 
    private void Awake()
    {
        mo = gameObject.GetComponent<MatrixObject>();
        ps = GetComponent<ParticleSystem>();
    }

    public void SetExplodeComponent(ExplodeOnDeath explodeComponent)
    {
        prevObjectExplodeComponent = explodeComponent;
    }

    public void ExplodeCell(bool isChainingChicken)
    {
        if (prevObject != null && prevObject.isCrushable)
            prevObject.SpriteRenderer.enabled = false;
        
        ps.Play();

        StartCoroutine(ExplodeCellCoroutine(isChainingChicken));
    }

    IEnumerator ExplodeCellCoroutine(bool isChainingChicken)
    {
        yield return new WaitForSeconds(0.5f);

        if (prevObjectExplodeComponent != null)
        {
            prevObjectExplodeComponent.Explode();
        }

        // 이전 오브젝트로인한 폭발 과정에 코루틴이 여기서 증발되는 걸 의도함
        yield return new WaitForSeconds(0.3f);
        
        MatrixCell currentCell = mo.GetCurrentCell();
        
        if (isChainingChicken)
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
        
        yield return new WaitForSeconds(0.2f);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if(prevObjectExplodeComponent != null)
            Destroy(prevObjectExplodeComponent.gameObject);
    }

    public void SetPrevObject(MatrixObject targetCellMatrixObject)
    {
        prevObject = targetCellMatrixObject;
    }
}
