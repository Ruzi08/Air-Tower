Shader "Custom/3D Text Occlusion" {
    Properties {
        _MainTex ("Font Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
    }

    SubShader {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        
        // Включаем Z-буфер
        ZWrite On
        ZTest LEqual
        Cull Off
        Lighting Off
        
        // Настройка блендинга для альфа-канала
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
            SetTexture [_MainTex] {
                constantColor [_Color]
                combine constant * texture
            }
        }
    }
}