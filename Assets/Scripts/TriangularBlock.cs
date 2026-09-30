using UnityEngine;

/// <summary>
/// Маркер треугольного числа-блока (1, 3, 6, 10, 15, 21, 28, 36, 45, 55...).
/// Треугольный блок НЕ является параллелепипедом, поэтому обычный компонент
/// <see cref="Scale"/> (где number = x*y*z) к нему неприменим — число хранится здесь напрямую.
///
/// step   — номер шага последовательности (k): 1,2,3...  (в ряду добавляется k единиц)
/// number — само треугольное число: k*(k+1)/2
/// </summary>
public class TriangularBlock : MonoBehaviour
{
    [Tooltip("Номер шага последовательности k (1,2,3...). На шаге k добавляется ряд из k единиц.")]
    public int step;

    [Tooltip("Треугольное число = k*(k+1)/2 (1,3,6,10,15,21,28,36,45,55...)")]
    public long number;

    public override string ToString() {
        return $"Triangular T{step} = {number}";
    }
}
