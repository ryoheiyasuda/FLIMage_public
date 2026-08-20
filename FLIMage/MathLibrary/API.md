# MathLibrary public API

This reference describes the public surface of `MathLibrary.dll`. It is organized by task rather than source order. Signatures use C# notation; generic numeric methods generally require `T : struct` and convert values through runtime numeric conversion.

## `Fitting.Nlinfit`

Bounded Levenberg-Marquardt nonlinear regression for one-dimensional or multi-coordinate observations.

### Construction and model delegates

```csharp
new Fitting.Nlinfit(double[] beta1, double[] x1, double[] y1)
new Fitting.Nlinfit(double[] beta1, double[,] xy, double[] z)
```

For the first constructor, assign `modelFunc` and optionally `modelFuncInPlace` and `jacobianFuncInPlace`. For the second, assign `modelFunc2` and optionally `modelFunc2InPlace`.

| Member | Meaning |
| --- | --- |
| `beta0` | Initial parameter vector. |
| `beta` | Final parameter vector after `Perform()`. |
| `betaMin`, `betaMax` | Inclusive parameter bounds; initialized to negative/positive infinity. |
| `fix` | Parameters to hold at their `beta0` values. |
| `x`, `x2`, `y` | Independent and observed data. |
| `weights` | Per-observation least-squares weights. |
| `modelFunc`, `modelFunc2` | Allocating model callbacks. |
| `modelFuncInPlace`, `modelFunc2InPlace` | Allocation-free model callbacks. |
| `jacobianFuncInPlace` | Analytic transposed Jacobian callback, `jt[parameter][observation]`. |
| `maxiter` | Maximum iterations; default `100`. |
| `rtol` | Objective convergence tolerance; default `1e-8`. |
| `poissonMaximumLikelihood` | Whether the objective is Poisson deviance. Prefer enabling it through `PoissonMaximumLikelihood()`. |
| `fitCurve`, `residual`, `Jt`, `xi_square` | Outputs populated by fitting/evaluation. |

### Methods

- `void PoissonWeights()` sets least-squares weights to `1 / sqrt(y)` for positive counts and `1` otherwise.
- `void PoissonMaximumLikelihood()` selects Poisson-deviance fitting and resets weights to one.
- `int Perform()` runs the fit. `0` indicates success; negative values indicate invalid input or numerical failure.
- `double GetSSE(double[] beta, out double[] r)` evaluates weighted SSE or Poisson deviance and returns its residual vector.
- `double[][] DirectJtJ(double[][] Jt)` calculates `J * J^T` from the transposed Jacobian.
- `double[] GetStep(double[] r, double lambda)` calculates a damped step from the current Jacobian.

`DirectJtJ` and `GetStep` expose solver internals and normally are not needed by callers.

## `ML_Fitting.ML_Fit`

Legacy nonlinear fitter with similar mutable fields and constructors:

```csharp
new ML_Fitting.ML_Fit(double[] beta1, double[] x1, double[] y1)
new ML_Fitting.ML_Fit(double[] beta1, double[,] xy, double[] z)
```

Public methods are `PoisonWeights()` (spelling preserved for compatibility), `Perform()`, `Get2lnLP(double[])`, `DirectJtJ(double[][])`, and `GetStep(double[], double)`. New code should normally use `Fitting.Nlinfit`, which includes the maintained Poisson maximum-likelihood path, in-place callbacks, bounds handling, and analytic Jacobian support.

## `ImageProcessing`

Unless stated otherwise, image axes are `[y, x]` and FLIM axes are `[y, x, time]`.

### Loading, layout, and spatial operations

- `ImportImage(string)` and `SaveImage(ushort[,], string)` load/save grayscale-style images.
- `EvenOddImage(...)` splits alternating source rows.
- `ShiftImage<T>(...)` translates a 2D image.
- `ImageSqrt(...)` and `ImageSmooth<T>(...)` transform intensity images.
- `PermuteFLIM5D(...)` switches channel/Z nesting for `ushort[][][,,]`; `deepCopy` controls whether FLIM volumes are cloned.
- `FLIM_Pages2FLIMRaw5D(...)` converts page-based FLIM layouts to raw 5D nesting.
- `getLinesFLIM(...)`, `CorrectEvenOddDifference(...)`, `flipImageX(...)`, and `MatrixCorrectDriftFLIM(...)` manipulate FLIM volumes.
- `MapImageInSquare<T>(...)` pads/remaps a jagged image to a square layout.

### Focus, registration, and Gaussian fitting

- `FitImageWithGaussian2D(...)`, `FitImageWithGaussian2D_NearPeak(...)`, and `Gaussian2D(...)` fit/evaluate a rotated 2D Gaussian.
- `GetFocusFrameByIntensity(...)` scores Z frames by intensity and can fit the peak; it also returns raw scores through `z_data`.
- `GetFocusFrame(...)` selects a focus frame using the FFT metric.
- `MeasureFocus_FFT(...)` accepts rectangular or jagged images.
- `MatrixMeasureDrift2D_FFT(...)` estimates drift using FFT cross-correlation.
- `MatrixMeasureDrift2D(...)` estimates drift over a requested calculation range.

### FLIM projection and lifetime

```csharp
ushort[,] GetProjectFromFLIM(ushort[,,] acqFImg, int[] t_range)

void GetProjectFromFLIMLines(
    ushort[,,] acqFImg,
    ushort[,] destination,
    int[] time_range,
    int startLine,
    int endLine)

float[,] GetLifetimeMapFromFLIM(
    ushort[,,] acqFImg,
    int[] trange,
    float psPerChannel,
    float offset)
```

All ranges are half-open. Projection sums time bins. The lifetime map computes the photon-weighted mean time in nanoseconds and subtracts `offset`; a zero-count pixel returns zero. `GetProjectFromFLIMLines` changes only destination rows in `[startLine, endLine)`.

### Color and bitmap rendering

`ColorScheme` values are `Spectrum`, `Fire`, `RB`, `YellowHighlight`, `YellowHighlight_Mod`, and `Plasma`.

`ColorBarDirection` values are `LeftToRight`, `RightToLeft`, `TopToBottom`, and `BottomToTop`.

- `ValueToRGB(...)`, `ValueToRGB_RB(...)`, `ValueToRGB_Fire(...)`, and `ValueToRGB_Jet(...)` create packed 3-byte color data.
- `FormatImageFLIM(...)` combines lifetime hue with intensity brightness and thresholding.
- `FormatImage(...)` renders rectangular or jagged intensity arrays.
- `FormatImageLines(...)` updates selected rows of an existing bitmap.
- `CreateColorBar(...)`, `MergeBitmaps(...)`, and `ResizeBitmap(...)` compose display images.
- `BitmapToByteArray(...)`, `PixelsToBitmap(...)`, and `PixelsToBitmapCopy(...)` convert bitmap storage. Both pixel-to-bitmap methods copy the supplied byte data, so the returned bitmap does not depend on the input array's lifetime.
- `GetMinInt(...)` returns the minimum value of a jagged `ushort` image.

The caller owns every returned `Bitmap` and must dispose it.

### Display helpers

- `ImShow(ushort[,])` and `ImShow(ushort[,], ushort[,])` show image windows.
- `Plot(double[])` and `Plot(double[], double[])` create and show an `Image_Showing_Window`, which is returned for further updates.

These methods require a Windows Forms UI environment.

### FLIM decay models and simulation

```csharp
double[] ExpGaussArray(double[] beta, double[] x, double pulseI)
void ExpGaussArrayInPlace(double[] beta, double[] x, double pulseI, double[] y)
void ExpGaussJacobianInPlace(double[] beta, double[] x, double pulseI, double[][] jt)

double[] Exp2GaussArray(double[] beta, double[] x, double pulseI)
void Exp2GaussArrayInPlace(double[] beta, double[] x, double pulseI, double[] y)
void Exp2GaussJacobianInPlace(double[] beta, double[] x, double pulseI, double[][] jt)
```

Single-component `beta` is `[amplitude, rate, gaussianSigma, timeOffset, baseline]`. Double-component `beta` is `[amplitude1, rate1, amplitude2, rate2, gaussianSigma, timeOffset, baseline]`. Periodic contributions are included until their estimated remainder is below `1e-4`.

`ExpGauss(double[] beta0, double x, double pulseI, int n_pulses)` evaluates the four-parameter single-component kernel `[amplitude, rate, gaussianSigma, timeOffset]` for an explicitly selected number of preceding pulses; it does not add a baseline.

`CreateFLIM_Sim(...)` creates a striped `[height, width, time]` biexponential volume. `SimRandomSeed` controls reproducibility and `SimUsePoissonNoise` selects Poisson sampling versus rounded deterministic values.

## `MatrixCalc`

`MatrixCalc` contains static utilities. The SIMD methods use hardware acceleration where available and fall back to scalar code. `IntelMKL_on` reports whether MathNet's native MKL provider was enabled.

### Numeric conversion and reductions

- Depth conversion: `changeDepthFrom16To8`, `changeDepthFrom8To16`.
- Min/max/sum: `calcMin`, `calcMax`, `calcSum` and their `_Normal` variants.
- Range sums: `calcSumFast`, `calcSumUshort`, `SIMD_VectorSum`, `SIMD_VectorSumUshort` and scalar variants.
- Conversion: `convertToFloat`, `convertToFloatSIMD`, `convertToFloat_Normal`, `convertToDouble_SIMD`, `convertToDouble_Normal`, `ConvertToFloatMatrix`, `FloatToByteVector`, and `FloatToByteMatrix`.
- Statistics: `Mean`, `Mean2D`, `Std2D`, `MatrixSum`, `MatrixSum_Matrix`, `Norm`, and `Prod`.
- Products: `ArrayDotProduct`, `ArrayDotProduct_Normal`, `ArrayDotProduct_SIMD_double`, `SIMD_Dot`, `Dot_withRange`, `Dot_withRange2`, `Dot_withRange_Normal`, and `Dot_withRange_SIMD`.

Methods with `Normal` in the name are explicit scalar/reference implementations. The unsuffixed high-throughput variants may choose SIMD.

### Shape, allocation, and copying

- `LinearizeArray`, `Linearize2DJaggedArray`, `CreateJaggedArrayFromLinearArray`, and `Reshape` convert storage layouts.
- `MatrixCreate2D` through `MatrixCreate5D` allocate nested jagged arrays.
- `MatrixCopy2D` through `MatrixCopy5D` deep-copy the nested levels represented by their signatures.
- `Matrix2Vector`, `ConvertToDoubleVector`, `ConvertToDoubleMatrix`, and `ConvertToComplexMatrix` bridge arrays and MathNet types.
- `ResizeArray2D`, `CopyFrom3DToLinear`, and `extract3rdAxis` copy or resize selected layouts.
- `SplitImage`, `GetSplitImage`, `TYX2XYT`, `XYT2TYX`, `makeNew5DSlice`, and `makeNew5DSlice_linear` handle application-specific multidimensional layouts.

### Element-wise calculations

`CalculationType` has `Add`, `Subtract`, `Multiply`, and `Max`.

- `ArrayCalc(...)` mutates its first array in place.
- `ArrayCalc_Normal(...)` mutates the selected half-open range of its first vector.
- `MatrixCalc2D(...)` and `MatrixCalc3D(...)` perform element-wise operations; overloads with `overwrite` control reuse of the first input.
- `Blanck3DMatrix(...)` (legacy spelling) returns a zero-valued array with the input shape.
- `MultiplyConstantToVector`, `DivideConstantFromVector`, `SubtractConstantFromVector`, `DivideConstantFromMatrix`, and `SubtractConstantFromMatrix` apply scalar operations.

### Linear algebra, FFT, and geometry

- `MatrixProduct(...)`, `MatrixTranspose(...)`, `MatrixInverse(...)`, `InverseMatrix(...)`, and `MatrixSolve(...)` provide basic linear algebra.
- `MatrixConvolution(...)` performs spatial convolution; `MatrixConvolution__FFT(...)` provides FFT overloads.
- `FFT2DForward(ref Matrix<Complex>)` and `FFT2DInverse(ref Matrix<Complex>)` transform a MathNet complex matrix.
- `Matrix_similarity(...)` and `xcorr(...)` calculate similarity/correlation.
- `Rotate(...)` rotates an XY pair by degrees. `RotateOffsetTimeSeries(...)` rotates, offsets, limits, flips, and optionally swaps a coordinate series in place.

### Models and special functions

- `linearRegression(x, y)` fits `y = intercept + slope * x` and returns `[intercept, slope]`.
- `Gaussian(...)`, `Gaussian_NoOffset(...)`, `FindPeak_WithGaussianFit1D(...)`, and `FindPeak_WithGaussianFit1D_NoOffset(...)` evaluate or fit 1D Gaussian models.
- `Sinusoidal(...)` and `InverseSinusoidal(...)` evaluate/invert the library's sinusoidal calibration model.
- `Erf(...)`, `Erfc(...)`, and `Erf_ry(...)` provide error-function implementations.
- `ExpGauss(double[] beta0, double x)` evaluates the non-periodic exponentially modified Gaussian helper in `MatrixCalc`.

## `GraphicCalc`

```csharp
bool GraphicCalc.isInsidePolygon(PointF[] polygon, int n, PointF point)
```

Returns `true` when the point is inside or on the boundary of the first `n` polygon vertices. At least three vertices are required. The implementation casts a ray toward `X = 10000`; coordinates at or beyond that sentinel can produce unreliable results.

## `Image_Showing_Window`

A Windows Forms viewer with these constructors:

```csharp
new Image_Showing_Window(ushort[,] image)
new Image_Showing_Window(Bitmap image)
new Image_Showing_Window(double[] x, double[] y)
```

`showImage(Bitmap)` replaces the displayed image and `AddPlot(double[], double[])` adds a curve. Dispose the form when it is no longer needed.
