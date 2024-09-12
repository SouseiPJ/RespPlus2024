# Correlation (C)

$$
C_r(\tau) = \frac{1}{N}\sum_{i} c_{x_{\tau},r}(i)
$$

ここで，

$$
c_{x_{\tau},r}(t) = x(t-\tau) \cdot r(t)
$$

# Mean Square Difference (MSD)

$$
{\mathrm MSD}_r(\tau) = \frac{1}{N}\sum_{i} SD_{x_{\tau},r}(i) $$

ここで，

$$
SD_{x_{\tau},r}(t) = \left\{x(t-\tau) - r(t)\right\}^2
$$

## Mean Absolute Difference (MAD)

$$
\mathrm{MAD}_r(\tau) = \frac{1}{N}\sum_{i} AD_{x_{\tau},r}(i)
$$

ここで，

$$
AD_{x_{\tau},r}(t) = \lvert x(t-\tau) - r(t) \rvert
$$

# iteration $i$ の範囲

$x(j-\tau)$, $r(j)$ が存在する範囲となる．

$x$:[0,Lx-1], $r$[0,Lr-1]とすると
$0 \leq j-\tau \leq L_x-1$ かつ $0 \leq j \leq L_r-1$ つまり，$\tau \leq j \leq \min (L_x+\tau-1,~ L_r-1)$ となる．

Type I: $j\in [\tau,L_x+\tau-1]$

Type II: $j\in [\tau,L_r-1]$

# CrossSpectrum

$$
C_r(\tau) = \mathtt{ifft}\left(\mathtt{fft}(x) \cdot \overline{\mathtt{fft}(r)}\right)
$$

$$
C_r^{(II)}(\tau) <=> \mathtt{ifft}\left( \mathtt{fft}(x^{\prime}) \cdot \overline{\mathtt{fft}(r)}\right)
$$
ここで
$$
x^{\prime} = \{x,zeros\} , L_x^{\prime} >= L_x + L_r
$$


# Autos (自己相関ライク)

## Auto Correlation (AC)

$$
{\mathrm AC}(\tau) = C_x(\tau)
$$

# Auto MAD



# Auto MSD = CMND: Cumulative mean normalized difference

Yin 提唱

$$
CMND_r(\tau) = \frac{MSD_r(\tau)}{\frac{1}{\tau}\sum_{j=0}^{\tau-1}MSD_r(j)}
$$

# Normalized Square Difference

$$
NSD_r(\tau) = \frac{C_r(\tau)}{M_r(\tau)}
$$

ここで，

$$
\begin{align}
M_r(\tau) &= \frac{1}{2} \left\{C_{x_\tau}(0) + C_r(0)\right\} \\
&= \frac{1}{2} \left\{ \frac{1}{N_i}\sum_{i}x(i-\tau)^2 + \frac{1}{N_j}\sum_{j}r(j)^2\right\}
\end{align}
$$


# 実装編

## Auto Correlation

```cs
private floor AutoCorr(x)
{
    // AutoCorrelation TypeII(sum_t:[0,N-\tau-1])
    // * inputs
    //   x (array)
    // * outputs
    //   R_x
    int NFFT = (int)(LenFFT*2);
    complex xp[NFFT]={0};
    complex Xp[NFFT],Rp[NFFT],rp[NFFT];
    float r[LenFFT];

    for(int i=0;i<LenFFT;i++) xp[i].real=x[i];
    Xp = FFT(xp,NFFT);
    for(int i=0;i<NFFT;i++) Rp[i] = Xp[i] * Xp[i].conj();
    rp = IFFT(Rp,NFFT);
    for(int i=0;i<LenFFT;i++) r[i] = rp[i].real();
    return r;
}
```

## Normalized Square Difference


```cs
private floor nsdf(x,r)
{

}
```


## MPM

```cs
    private floor MPM(r)
    {
        // McLeod Pitch Method (MPM)
        // * inputs
        //   r (array): AutoCorrelation or so on.
        // * outputs
        //   ret (array)
        //     ret[0]: estimated pitch
        //     ret[2]: clarity of estimation
        int k = 0;
        for(int i=0;i<LenFFT-1;i++){
            if(r[i]<0 && r[i+1]>0){
                vpeak = 0;
                ipeak = 0;
                for(int j=i;;j++){
                    if(r[j] > vpeak){
                        vpeak = r[j];
                        ipeak = j;
                    }
                    if(r[j] < 0){
                        break;
                    }
                }
                vpeaks[k] = vpeak;
                ipeaks[k] = ipeak;
                k++;
            }
        }
        npeaks = k;
        v_maxpeak = 0;
        for(int i=0;i<npeaks;i++){
            if(vpeaks[i] > v_maxpeak){
                v_maxpeak = vpeaks[i];
            }
        }
        for(int i=0;i<npeaks;i++){
            if(vpeaks[i] > 0.8*v_maxpeak){
                tau_peak = ipeaks[i];
                pitch = 1/tau_peak;
                clarity = vpeaks[i];
                break;
            }
        }
        ret[0] = pitch;
        ret[1] = clarity;
        return ret;
    }
```
