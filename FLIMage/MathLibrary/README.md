# MathLibrary.dll

`MathLibrary.dll` is FLIMage's managed numerical and image-processing library. It provides nonlinear curve fitting, FLIM projection and lifetime-map operations, image rendering, matrix/array utilities, FFT-based measurements, and small Windows Forms visualization helpers.

The assembly namespace is `MathLibrary` and the project targets .NET Framework 4.8. See [API.md](API.md) for the public API grouped by feature.

## Referencing the library

Inside this repository, prefer a project reference:

```xml
<ProjectReference Include="..\MathLibrary\MathLibrary.csproj" />
```

For an external .NET Framework project, add references to:

- `MathLibrary.dll`
- `Utilities.dll`
- `MathNet.Numerics.dll`
- `System.Numerics.Vectors.dll`

Keep the dependency DLLs beside the executable. The library also uses `System.Drawing` and `System.Windows.Forms`; image-display methods therefore require Windows and an interactive desktop.

```csharp
using MathLibrary;
```

## Build

From the repository root, build the supported x64 configuration with Visual Studio MSBuild:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" `
  ".\FLIMage\MathLibrary\MathLibrary.csproj" /t:Build /p:Configuration=Release /p:Platform=x64
```

Change the Visual Studio edition in the path when necessary. The output is written to `FLIMage\MathLibrary\bin\Release`. Building the full solution also copies the DLL to the application's external-library folder.

## Data conventions

- A FLIM image is normally `ushort[,,]` indexed as `[y, x, time]`.
- A projected intensity image is `ushort[,]` indexed as `[y, x]`.
- Time and line ranges use the half-open form `[start, end)`. For example, `{ 1, 4 }` includes bins 1, 2, and 3.
- `GetLifetimeMapFromFLIM` accepts `psPerChannel` in picoseconds and returns lifetime values in nanoseconds after subtracting `offset`.
- Many methods accept both rectangular arrays (`T[,]`) and jagged arrays (`T[][]`). These representations are not interchangeable.
- Rendering methods return `Bitmap` objects owned by the caller. Dispose them with `using` or `Dispose()`.
- Several high-throughput array operations mutate their first argument. Check [API.md](API.md) before assuming a returned copy.

## Common examples

### Project a FLIM image and calculate a mean-arrival-time map

```csharp
ushort[,,] flim = LoadFlimData(); // [y, x, time]
int[] timeRange = { 4, 60 };

ushort[,] intensity = ImageProcessing.GetProjectFromFLIM(flim, timeRange);
float[,] lifetimeNs = ImageProcessing.GetLifetimeMapFromFLIM(
    flim,
    timeRange,
    psPerChannel: 50.0f,
    offset: 0.25f);

using (var display = ImageProcessing.FormatImageFLIM(
    intensity_range: new double[] { 0, 2000 },
    FLIM_range: new double[] { 0.5, 4.0 },
    thresh_highlow: new double[] { 20, -1 },
    FLIMImg: lifetimeNs,
    AcqImg: intensity,
    forceSquare: false,
    color_scheme: ImageProcessing.ColorScheme.Plasma))
{
    display.Save("lifetime.png");
}
```

Projection sums are accumulated as integers and stored as `ushort`; very bright projections can wrap when converted to `ushort`.

### Fit a periodic single-exponential Gaussian response

The parameter order is:

```text
single exponential: [amplitude, rate, gaussianSigma, timeOffset, baseline]
double exponential: [amplitude1, rate1, amplitude2, rate2,
                     gaussianSigma, timeOffset, baseline]
```

`rate` is the reciprocal lifetime in the same bin-based time units as `x`. `pulseInterval` is also expressed in bins.

```csharp
double pulseInterval = 80.0;
double[] x = Enumerable.Range(0, observed.Length)
                       .Select(i => (double)i)
                       .ToArray();

double[] initial = { observed.Max(), 0.05, 0.5, 5.0, 0.0 };
var fit = new Fitting.Nlinfit(initial, x, observed)
{
    modelFunc = (beta, values) =>
        ImageProcessing.ExpGaussArray(beta, values, pulseInterval),
    modelFuncInPlace = (beta, values, output) =>
        ImageProcessing.ExpGaussArrayInPlace(beta, values, pulseInterval, output),
    jacobianFuncInPlace = (beta, values, jacobian) =>
        ImageProcessing.ExpGaussJacobianInPlace(beta, values, pulseInterval, jacobian)
};

fit.betaMin[0] = 0.0;   // amplitude
fit.betaMin[1] = 0.001; // rate
fit.betaMin[2] = 0.05;  // Gaussian sigma
fit.betaMin[4] = 0.0;   // baseline
fit.PoissonMaximumLikelihood();

int status = fit.Perform();
if (status != 0)
    throw new InvalidOperationException($"Fit failed with status {status}.");

double[] parameters = fit.beta;
double[] fittedCurve = fit.fitCurve;
```

Use `PoissonMaximumLikelihood()` for photon-count histograms, especially sparse data. `PoissonWeights()` provides weighted least squares with `1/sqrt(count)` weights instead. Supplying the in-place model and analytic Jacobian is optional but avoids allocations and finite-difference work.

### Reproducible simulated FLIM data

```csharp
ImageProcessing.SimRandomSeed = 1234;
ImageProcessing.SimUsePoissonNoise = true;

double[] biexponential = { 100, 0.05, 40, 0.2, 0.5, 5.0, 0.0 };
ushort[,,] simulated = ImageProcessing.CreateFLIM_Sim(
    n_dtime: 128,
    height: 64,
    width: 64,
    beta2: biexponential,
    pulseI: 80.0,
    n_stripe: 4);
```

Set `SimRandomSeed` to `null` to use a time-dependent seed. Setting `SimUsePoissonNoise` to `false` returns rounded, noise-free values clipped to the `ushort` range.

## Important behavior

- `Fitting.Nlinfit.Perform()` returns `0` on success and a negative value on failure. Results are written to `beta`, `fitCurve`, `residual`, and `xi_square`.
- `betaMin`, `betaMax`, and `fix` are initialized by the constructors. A `true` entry in `fix` holds that parameter at its initial value.
- The fitting constructors retain the supplied arrays; clone inputs first if shared mutable state would be unsafe.
- In-place model functions must fill an output array whose length equals the observation count. Jacobians are transposed: `jt[parameter][observation]`.
- `GetLifetimeMapFromFLIM` and projection methods clamp the supplied time-range array in place.
- `ImportImage` reads only the red channel into a `ushort[,]`; `SaveImage` creates an 8-bit-style grayscale `Bitmap` through `Color.FromArgb`, so values must be valid color-channel values.
- `ImShow`, `Plot`, and `Image_Showing_Window` pump Windows Forms events and should only be called from an appropriate UI context.
- `MatrixCalc.IntelMKL_on` reports whether MathNet successfully enabled its native MKL provider during type initialization; code must continue to work when it is `false`.

## Source layout

- `Fitting.cs` — current Levenberg-Marquardt fitter, including Poisson maximum likelihood.
- `ML_Fitting.cs` — older maximum-likelihood fitter retained for compatibility.
- `ImageProcessing.cs` — FLIM operations, drift/focus calculations, rendering, simulation, and decay models.
- `MatrixCalc.cs` — array, matrix, statistics, SIMD, FFT, and model helpers.
- `GraphicCalc.cs` — point-in-polygon test.
- `Image_Showing_Window.cs` — Windows Forms image/plot viewer.
