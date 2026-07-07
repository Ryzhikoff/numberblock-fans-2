using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMaster : MonoBehaviour {

    public AudioClip clipOne;
    public AudioClip clipOneForBig;
    public AudioClip clipTen;
    public AudioClip clipThousand;
    public AudioClip clipHundred;
    public AudioClip clipMillion;
    public AudioClip clipBillion;
    public AudioClip clipTrillion;

    public AudioClip clipQuadrillion;
    public AudioClip clipQuintillion;
    public AudioClip clipSextillion;
    public AudioClip clipSeptillion;
    public AudioClip clipOctillion;
    public AudioClip clipNonillion;
    public AudioClip clipDecillion;

    public AudioClip clipUndecillion;
    public AudioClip clipDuodecillion;
    public AudioClip clipTredecillion;
    public AudioClip clipQuattuordecillion;
    public AudioClip clipQuindecillion;
    public AudioClip clipSexdecillion;
    public AudioClip clipSeptendecillion;
    public AudioClip clipOctodecillion;
    public AudioClip clipNovemdecillion;
    public AudioClip clipVigintillion;
    public AudioClip clipUnvigintillion;
    public AudioClip clipDuovigintillion;
    public AudioClip clipTrevigintillion;
    public AudioClip clipQuattuorvigintillion;
    public AudioClip clipQuinvigintillion;
    public AudioClip clipSexvigintillion;
    public AudioClip clipSeptenvigintillion;
    public AudioClip clipOctovigintillion;
    public AudioClip clipNovemvigintillion;
    public AudioClip clipTrigintillion;
    public AudioClip clipUntrigintillion;
    public AudioClip clipDuotrigintillion;
    public AudioClip clipTrestrigintillion;
    public AudioClip clipQuattuortrigintillion;
    public AudioClip clipQuintrigintillion;
    public AudioClip clipSextrigintillion;
    public AudioClip clipSeptentrigintillion;
    public AudioClip clipOctotrigintillion;
    public AudioClip clipNovemtrigintillion;
    public AudioClip clipQuadragintillion;
    public AudioClip clipUnquadragintillion;
    public AudioClip clipDuoquadragintillion;
    public AudioClip clipTrequadragintillion;
    public AudioClip clipQuattuorquadragintillion;
    public AudioClip clipQuinquadragintillion;
    public AudioClip clipSexquadragintillion;
    public AudioClip clipSeptenquadragintillion;
    public AudioClip clipOctoquadragintillion;
    public AudioClip clipNovemquadragintillion;
    public AudioClip clipQuinquagintillion;
    public AudioClip clipUnquinquagintillion;
    public AudioClip clipDuoquinquagintillion;
    public AudioClip clipTrequinquagintillion;
    public AudioClip clipQuattuorquinquagintillion;
    public AudioClip clipQuinquinquagintillion;
    public AudioClip clipSexquinquagintillion;
    public AudioClip clipSeptenquinquagintillion;
    public AudioClip clipOctoquinquagintillion;
    public AudioClip clipNovemquinquagintillion;
    public AudioClip clipSexagintillion;
    public AudioClip clipUnsexagintillion;
    public AudioClip clipDuosexagintillion;
    public AudioClip clipTresexagintillion;
    public AudioClip clipQuattuorsexagintillion;
    public AudioClip clipQuinsexagintillion;
    public AudioClip clipSexsexagintillion;
    public AudioClip clipSeptensexagintillion;
    public AudioClip clipOctosexagintillion;
    public AudioClip clipNovemsexagintillion;
    public AudioClip clipSeptuagintillion;
    public AudioClip clipUnseptuagintillion;
    public AudioClip clipDuoseptuagintillion;
    public AudioClip clipTreseptuagintillion;
    public AudioClip clipQuattuorseptuagintillion;
    public AudioClip clipQuinseptuagintillion;
    public AudioClip clipSexseptuagintillion;
    public AudioClip clipSeptenseptuagintillion;
    public AudioClip clipOctoseptuagintillion;
    public AudioClip clipNovemseptuagintillion;
    public AudioClip clipOctogintillion;
    public AudioClip clipUnoctogintillion;
    public AudioClip clipDuooctogintillion;
    public AudioClip clipTreoctogintillion;
    public AudioClip clipQuattuoroctogintillion;
    public AudioClip clipQuinoctogintillion;
    public AudioClip clipSexoctogintillion;
    public AudioClip clipSeptoctogintillion;
    public AudioClip clipOctooctogintillion;
    public AudioClip clipNovemoctogintillion;
    public AudioClip clipNonagintillion;
    public AudioClip clipUnnonagintillion;
    public AudioClip clipDuononagintillion;
    public AudioClip clipTrenonagintillion;
    public AudioClip clipQuattuornonagintillion;
    public AudioClip clipQuinnonagintillion;
    public AudioClip clipSexnonagintillion;
    public AudioClip clipSeptennonagintillion;
    public AudioClip clipOctononagintillion;
    public AudioClip clipNovemnonagintillion;
    public AudioClip clipCentillion;
    public AudioClip clipGoogol;

    public AudioClip clipDownSound;
    public AudioClip clipBoom;
    public AudioClip mainTheme;

    public float volumeVoice;
    public float volumeBoom;
    public float volumeDown;
    public float volumeMainTheme;
    public float pitchVoiceSound;
    public float pitchMainTheme;

    public bool stopMainTheme = true;

    private static AudioSource audio1;
    private static AudioSource audio2;
    private static AudioSource audio3;

    private static AudioSource audioBoom;
    private static AudioSource audioDown;
    private static AudioSource audioMainTheme;

    private static bool auido1isStarting = false;
    private static bool auido2isStarting = false;
    private static bool auido3isStarting = false;

    private static bool isDozens = false;

    private static int counter = 1;

    //для старого режима - без генерации длинных звуков
    private bool _oldMode = false;

    /// <summary>
    /// false - по умолчанию, новый режим, с генерацией сложных звуков
    /// true - старый режим, точного воспроизведения звуков
    /// </summary>
    public bool oldMode {
        get {
            return _oldMode;
        }
        set {
            _oldMode = value;
        }
    }

    private void Start() {
        audio1 = gameObject.AddComponent<AudioSource>();
        audio2 = gameObject.AddComponent<AudioSource>();
        audio3 = gameObject.AddComponent<AudioSource>();

        audioBoom = gameObject.AddComponent<AudioSource>();
        audioBoom.clip = clipBoom;
        audioBoom.volume = volumeBoom;

        audioDown = gameObject.AddComponent<AudioSource>();
        audioDown.clip = clipDownSound;
        audioDown.volume = volumeDown;

        audioMainTheme = gameObject.AddComponent<AudioSource>();
        audioMainTheme.clip = mainTheme;
        audioMainTheme.volume = volumeMainTheme;
        audioMainTheme.loop = false;

        audioMainTheme.Play();

        //установка тональностей
        setPitchVoiceSound(pitchVoiceSound);
        setPitchMainTheme(pitchMainTheme);

        setVolumeVoice(volumeVoice);
    }

    private void Update() {


        //если установлено аудио в 1 - идем дальше
        if (audio1.clip != null) {
            //не играет в данный момент
            if (!audio1.isPlaying) {
                //не запускалась - запускаем
                if (!auido1isStarting) {
                    //print("надо запустить 1");
                    audio1.Play();
                    auido1isStarting = true;
                }
            }
            //не играет, но запускалась - удаляем
            if (!audio1.isPlaying && auido1isStarting) {
                audio1.clip = null;
                auido1isStarting = false;
            }

        }

        //не равно 0 и не играт саунд 1
        if (audio2.clip != null && !audio1.isPlaying) {
            //не играет в данный момент
            if (!audio2.isPlaying) {
                //не запускалась - запускаем
                if (!auido2isStarting) {
                    //print("надо запустить 2");
                    audio2.Play();
                    auido2isStarting = true;
                }
            }
            //не играет, но запускалась - удаляем
            if (!audio2.isPlaying && auido2isStarting) {
                audio2.clip = null;
                auido2isStarting = false;
            }
        }

        //не равно 0 и не играт саунд 1 и 2
        if (audio3.clip != null && !audio1.isPlaying && !audio2.isPlaying) {
            //не играет в данный момент
            if (!audio3.isPlaying) {
                //не запускалась - запускаем
                if (!auido3isStarting) {
                    //print("надо запустить 3");
                    audio3.Play();
                    auido3isStarting = true;
                }
            }
            //не играет, но запускалась - удаляем
            if (!audio3.isPlaying && auido3isStarting) {
                audio3.clip = null;
                auido3isStarting = false;
            }
        }

        //цикличность главной темы
        if (!audioMainTheme.isPlaying) {
            audioMainTheme.Play();
        }

        //контроль главной темы
        if (stopMainTheme) {
            if (audio1.isPlaying || audio2.isPlaying || audio3.isPlaying) {
                if (audioMainTheme.isPlaying) {
                    audioMainTheme.Pause();
                }
            } else {
                if (!audioMainTheme.isPlaying) {
                    audioMainTheme.Play();
                }
            }
        }
    }
    //установка тональности для озвучки
    public void setPitchVoiceSound(float pith) {
        audio1.pitch = pith;
        audio2.pitch = pith;
        audio3.pitch = pith;
    }
    //установка тональности для главной темы
    public void setPitchMainTheme(float pith) {
        audioMainTheme.pitch = pith;
    }
    public void playSound() {
        playSound(counter);
        counter++;
    }

    public void playSound(int counter) {

        switch (counter) {
            case 1:
                audio1.clip = clipOne;
                break;
            case 2:
                audio1.clip = clipTen;
                break;
            case 3:
                audio1.clip = clipOneForBig;
                audio2.clip = clipHundred;
                break;
            //62 - googol;
            /*case 102:
                audioMainTheme.clip = clipBoom;
                audio1.clip = clipGoogol;
                break;*/
            default:
                if (counter % 3 == 0) {
                    //print("зашли в блок для сотен counter%3 = " + counter / 3);
                    //сотни
                    audio1.clip = clipOneForBig;
                    audio2.clip = clipHundred;
                    audio3.clip = getClip((counter - 1) / 3);
                } else if (isDozens) {
                    //print("зашли в блок для десяток counter%3 = " + counter /2);
                    //десятки
                    audio1.clip = clipTen;
                    audio2.clip = getClip((counter - 1) / 3);
                    isDozens = !isDozens;
                } else {
                    //print("зашли в блок для единиц counter%3 = " + counter);
                    //единицы
                    audio1.clip = clipOneForBig;
                    audio2.clip = getClip((counter - 1) / 3);
                    isDozens = !isDozens;
                }
                break;
        }

        string t = "";
        if (audio1.clip != null) {
            t = t + " clip1: " + audio1.clip.name;
        }
        if (audio2.clip != null) {
            t = t + " clip2: " + audio2.clip.name;
        }
        if (audio3.clip != null) {
            t = t + " clip3: " + audio3.clip.name;
        }
        //print("counter: " + counter + t);
        
    }

    public void setVolumeVoice(float volume) {
        audio1.volume = volume;
        audio2.volume = volume;
        audio3.volume = volume;
    }

    private AudioClip getClip(int i) {
        return i switch {
            1 => clipThousand,
            2 => clipMillion,
            3 => clipBillion,
            4 => clipTrillion,
            5 => clipQuadrillion,
            6 => clipQuintillion,
            7 => clipSextillion,
            8 => clipSeptillion,
            9 => clipOctillion,
            10 => clipNonillion,
            11 => clipDecillion,
            12 => clipUndecillion,
            13 => clipDuodecillion,
            14 => clipTredecillion,
            15 => clipQuattuordecillion,
            16 => clipQuindecillion,
            17 => clipSexdecillion,
            18 => clipSeptendecillion,
            19 => clipOctodecillion,
            20 => clipNovemdecillion,
            21 => clipVigintillion,
            22 => clipUnvigintillion,
            23 => clipDuovigintillion,
            24 => clipTrevigintillion,
            25 => clipQuattuorvigintillion,
            26 => clipQuinvigintillion,
            27 => clipSexvigintillion,
            28 => clipSeptenvigintillion,
            29 => clipOctovigintillion,
            30 => clipNovemvigintillion,
            31 => clipTrigintillion,
            32 => clipUntrigintillion,
            33 => clipDuotrigintillion,
            34 => clipTrestrigintillion,
            35 => clipQuattuortrigintillion,
            36 => clipQuintrigintillion,
            37 => clipSextrigintillion,
            38 => clipSeptentrigintillion,
            39 => clipOctotrigintillion,
            40 => clipNovemtrigintillion,
            41 => clipQuadragintillion,
            42 => clipUnquadragintillion,
            43 => clipDuoquadragintillion,
            44 => clipTrequadragintillion,
            45 => clipQuattuorquadragintillion,
            46 => clipQuinquadragintillion,
            47 => clipSexquadragintillion,
            48 => clipSeptenquadragintillion,
            49 => clipOctoquadragintillion,
            50 => clipNovemquadragintillion,
            51 => clipQuinquagintillion,
            52 => clipUnquinquagintillion,
            53 => clipDuoquinquagintillion,
            54 => clipTrequinquagintillion,
            55 => clipQuattuorquinquagintillion,
            56 => clipQuinquinquagintillion,
            57 => clipSexquinquagintillion,
            58 => clipSeptenquinquagintillion,
            59 => clipOctoquinquagintillion,
            60 => clipNovemquinquagintillion,
            61 => clipSexagintillion,
            62 => clipUnsexagintillion,
            63 => clipDuosexagintillion,
            64 => clipTresexagintillion,
            65 => clipQuattuorsexagintillion,
            66 => clipQuinsexagintillion,
            67 => clipSexsexagintillion,
            68 => clipSeptensexagintillion,
            69 => clipOctosexagintillion,
            70 => clipNovemsexagintillion,
            71 => clipSeptuagintillion,
            72 => clipUnseptuagintillion,
            73 => clipDuoseptuagintillion,
            74 => clipTreseptuagintillion,
            75 => clipQuattuorseptuagintillion,
            76 => clipQuinseptuagintillion,
            77 => clipSexseptuagintillion,
            78 => clipSeptenseptuagintillion,
            79 => clipOctoseptuagintillion,
            80 => clipNovemseptuagintillion,
            81 => clipOctogintillion,
            82 => clipUnoctogintillion,
            83 => clipDuooctogintillion,
            84 => clipTreoctogintillion,
            85 => clipQuattuoroctogintillion,
            86 => clipQuinoctogintillion,
            87 => clipSexoctogintillion,
            88 => clipSeptoctogintillion,
            89 => clipOctooctogintillion,
            90 => clipNovemoctogintillion,
            91 => clipNonagintillion,
            92 => clipUnnonagintillion,
            93 => clipDuononagintillion,
            94 => clipTrenonagintillion,
            95 => clipQuattuornonagintillion,
            96 => clipQuinnonagintillion,
            97 => clipSexnonagintillion,
            98 => clipSeptennonagintillion,
            99 => clipOctononagintillion,
            100 => clipNovemnonagintillion,
            101 => clipCentillion,
            _ => null,
        };
    }

    public void playBoom() {
        audioBoom.Play();
    }

    public void boomSoundUp() {
        boomSoundUp(0.1f);
    }
    public void boomSoundUp(float value) {
        audioBoom.volume += value;
    }

    public void boomSoundDown() {
        boomSoundDown(0.1f);
    }
    public void boomSoundDown(float value) {
        audioBoom.volume -= value;
    }

    public void playDownSound(bool play) {
        if (play) {
            audioDown.Play();
        } else {
            audioDown.Stop();
        }
    }

}
